using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[Serializable]
public class OrientationBlockData
{
    public OrientationBlockConfig saveDataUsed;
    public string participantID = "0000";
    public int blockID = 0;
    public int trialCount = 0; // This should be equivalent to the number of trialTimes
    public List<float> trialTimesMiliseconds = new();
    public List<LineOrientation> selectedOrientation = new();

    public List<LineOrientation> actualOrientation = new();
    public List<Pose> viewPoses = new();
    public List<long> viewPosesTime = new();
    public List<bool> hadDistractor = new();
}

public enum LineOrientation
{
    Horizontal,
    Vertical
}

public enum TrialEvent
{
    Nothing,
    TriggerPressed,
    ButtonPressed, 
}

public class OrientationTrials : MonoBehaviour
{

    public static System.Diagnostics.Stopwatch stopwatch;
    private static long time_ms = 0;
    private static long start_time_ms = 0;
    private static LineOrientation itemOrientation;
    [HideInInspector]
    public static OrientationBlockData trials;
    public static bool isTrialRunning = false;
    public static bool isDataRecording = false;
    private static bool trialHadDistractor = false;
    [HideInInspector]
    public static GameObject trialObjectsParent;
    [HideInInspector]
    public static ConfigOptions configOptions;
    [HideInInspector]
    public static Camera vrCamera;
    
    
    //private static float radius;
    private static GameObject[] objects;
    public static GameManager gameManager;

    // (Miliseconds, Trial Events for if something happened, Hand Positions at that time) 50Hz
    public static (List<long>, List<TrialEvent>, List<Vector3>) mainTrialData = (new(), new(), new());
    // (Miliseconds, Eye Pose at that time) 120Hz
    public static (List<long>, List<Pose>) viewTrialData = (new(), new());





    public static void BlockStart(GameManager manager, OrientationBlockConfig config)
    {
        if (manager.isStartLockedOut)
        {
            return;
        }
        stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        manager.isStartLockedOut = true;
        manager.trial = Trial.Orientation;
        trials = new OrientationBlockData();

        trials.participantID = manager.participantID;
        

        vrCamera = manager.vrCamera;
        trialObjectsParent = manager.trialObjectsParent;

        SetupBlock(manager, config);

        manager.StartCoroutine( WaitForTrial(manager, true, config) );

    }

    public static void SetupBlock(GameManager manager, OrientationBlockConfig config) {
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        foreach (GameObject wall in walls)
        {
            wall.GetComponent<Renderer>().material.color = config.backgroundColor;
        }
    }



    public static IEnumerator WaitForTrial(GameManager manager, bool wait, OrientationBlockConfig config)
    {   
        if (wait)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(config.timeToSpawnMin, config.timeToSpawnMax));
            RunOrientationTrial(manager, config);
        }
        yield return new WaitForSeconds(0);
        RunOrientationTrial(manager, config);
    }

    public static IEnumerator TimeoutBreak(GameManager manager, int waitTime, OrientationBlockConfig config)
    {
        // Put on text to wait
        manager.isStartLockedOut = true;
        GameObject[] uiTextArr = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(sr => sr.name == "StartText").ToArray();
        GameObject uiText = uiTextArr[0];

        uiText.SetActive(true);
        uiText.GetComponent<TMPro.TextMeshProUGUI>().text = "Please take a break for " + waitTime + " seconds. Press Trigger Button after screen clears.";
        yield return new WaitForSeconds(waitTime);
        uiText.SetActive(false);

        manager.isStartLockedOut = false;
        manager.configOptions.GetNextBlockConfig();        
        // Remove text to wait

    }
    public static void RunOrientationTrial(GameManager manager, OrientationBlockConfig config)
    {
        if (isTrialRunning == true)
        {
            // Wait until the trial is over before starting a new one
            return;
        }
        isTrialRunning = true;
        manager.isTrialRunning = true;

        // Get the camera position and forward vector
        Vector3 cameraPos = manager.vrCamera.transform.position;
        Vector3 spawnPos = GameObject.FindGameObjectsWithTag("ItemSpawn")[0].transform.position;
        
        // Calculate the vector from the camera to the center of the spawn point
        Vector3 cameraForward = (spawnPos - cameraPos).normalized * config.distanceFromUserMeters;
        Vector3 shapeCenter = cameraPos + cameraForward;

        (Vector3[], GameObject[], int) pointsData = manager.GenerateShapePoints(config);
        // Extracting points Data
        Vector3[] objectPositions = pointsData.Item1;
        GameObject[] trialObjects = pointsData.Item2;
        int numberOfObjects = pointsData.Item3;



        // Shuffle the positions of the objects to randomize study
        var rng = new System.Random();
        GameManager.Shuffle(rng, objectPositions);

        // Get the starting time
        start_time_ms = stopwatch.ElapsedMilliseconds;
        

        // Start capturing data
        mainTrialData = (new(), new(), new());
        viewTrialData = (new(), new());
        isDataRecording = true;

        Color normalColor = config.regularItemColor;
        Color distractorColor = config.distracterItemColor;


        if (config.randomizeColors)
        {
            Color[] colors = {normalColor, distractorColor};
            GameManager.Shuffle(rng, colors);
            normalColor = colors[0];
            distractorColor = colors[1];
        }
        // Summon The Objects
        for (int i = 0; i < objectPositions.Length; i++)
        {
            GameObject prefabObj = trialObjects[i];
            GameObject obj = Instantiate(prefabObj, objectPositions[i] + shapeCenter, prefabObj.transform.rotation).gameObject;
            obj.transform.SetParent(trialObjectsParent.transform);
            obj.transform.localScale *= config.itemsScale;


            if (i == 0)
            {
                
                obj.GetComponent<Renderer>().material.color = normalColor;
                
                itemOrientation = RandLineOrientation(obj, manager.verticalMaterial, manager.horizontalMaterial);
            }
            else if (i < config.distractorObjects.Length + 1)
            {
                if (UnityEngine.Random.Range(0, 100) < config.randomPercentageOfDistractor)
                {
                    trialHadDistractor = true;
                    obj.GetComponent<Renderer>().material.color = distractorColor;
                    RandLineOrientation(obj, manager.verticalMaterial, manager.horizontalMaterial);
                }
                else
                {
                    trialHadDistractor = false;
                    obj.GetComponent<Renderer>().material.color = normalColor;
                    RandLineOrientation(obj, manager.verticalMaterial, manager.horizontalMaterial);
                }

            }
            else
            {
                obj.GetComponent<Renderer>().material.color = normalColor;

                RandLineOrientation(obj, manager.verticalMaterial, manager.horizontalMaterial);
            }
        }
        
    }


    public static void RecordTrialData(GameManager manager){
        
        // Format for text main file:
        // time, eventCode, Hand Position
        // Ex. 000001, eventCode, (0,0,0)

        // Format for text view file:
        // Time, Eye Position, Eye Rotation?
        // Ex. 000001, (0, 0, 0), (0,0,0)

        string mainPath = "./data/main_" + OrientationTrials.trials.participantID + "_" + OrientationTrials.trials.blockID + "_" + OrientationTrials.trials.trialCount + ".txt";
        string mainTrialcontent = "";
        string mainTrialInfo = (int)OrientationTrials.trials.selectedOrientation[OrientationTrials.trials.trialCount] + ", " + (int)OrientationTrials.trials.actualOrientation[OrientationTrials.trials.trialCount] + ", " + OrientationTrials.trials.hadDistractor[OrientationTrials.trials.trialCount] + "\n";
        string eyePath = "./data/eye_" + OrientationTrials.trials.participantID + "_" + OrientationTrials.trials.blockID + "_" + OrientationTrials.trials.trialCount + ".txt";
        string eyeTrialContent = "";

        for (int i = 0; i < mainTrialData.Item1.Count; i++)
        {
            mainTrialcontent += mainTrialData.Item1[i] + ", " + mainTrialData.Item2[i] + ", " + mainTrialData.Item3[i] + "\n";
        }

        for (int i = 0; i < viewTrialData.Item1.Count; i++)
        {
            eyeTrialContent += viewTrialData.Item1[i] + ", " + viewTrialData.Item2[i].position + ", " + viewTrialData.Item2[i].rotation + "\n";
        }

        if (!File.Exists("./data")) {
            Directory.CreateDirectory("./data");
        }

        if (!File.Exists(eyePath)) {
            File.WriteAllText(eyePath, eyeTrialContent);
        }else{
            File.AppendAllText(eyePath, eyeTrialContent);
        }

        if (!File.Exists(mainPath)) {
            File.WriteAllText(mainPath, mainTrialInfo);
        }

        File.AppendAllText(mainPath, mainTrialcontent);
        

    }
    
    public static IEnumerator StopRecordingDataDelay(float delay){
        yield return new WaitForSeconds(delay);
        isDataRecording = false;
    }

    public static void StopOrientationTrial(GameManager manager, LineOrientation orientation, OrientationBlockConfig config)
    {   
        // Get the ending time
        long end_time_ms = stopwatch.ElapsedMilliseconds;
        // Calculate the time taken
        time_ms = end_time_ms - start_time_ms;

        // Print the time taken
        Debug.Log("Time taken: " + time_ms + "ms");
        
        isTrialRunning = false;
        manager.isTrialRunning = false;
        manager.StartCoroutine(StopRecordingDataDelay(1f));
        


        trials.trialTimesMiliseconds.Add(time_ms);
        bool wasCorrect = orientation == itemOrientation;
        trials.actualOrientation.Add(itemOrientation);
        trials.selectedOrientation.Add(orientation);
        trials.hadDistractor.Add(trialHadDistractor);
        RecordTrialData(manager);
        
        trials.trialCount++;


        // Destroy all objects
        foreach (Transform transform in trialObjectsParent.transform)
        {
            Destroy(transform.gameObject);
        }
        
        if (trials.trialCount >= config.numberOfTrials)
        {
            if (manager.configOptions.IsLastBlock()){
                
                BlockEnd(manager, config);
                manager.EndStudy();                
            }
            else
            {
                BlockEnd(manager, config);
            
                manager.StartCoroutine(TimeoutBreak(manager, 30, config));
            }
        }
        
        else
        {
            manager.StartCoroutine( WaitForTrial(manager, true, config));
        }
    }

    public static LineOrientation RandLineOrientation(GameObject obj, Material verticalMaterial, Material horizontalMaterial)
    {
        LineOrientation orientation = (LineOrientation) UnityEngine.Random.Range(0, 2);


        if (orientation == LineOrientation.Horizontal)
        {
            if (obj.GetComponent<LineRenderer>() != null)
            {
                Bounds bounds = obj.GetComponent<Renderer>().bounds;
                Vector3 size = bounds.size;

                obj.GetComponent<LineRenderer>().SetPosition(0, new Vector3(-1 * size.x / obj.gameObject.transform.localScale.x / 2.5f, 0, 0));
                obj.GetComponent<LineRenderer>().SetPosition(1, new Vector3(size.x / obj.gameObject.transform.localScale.x / 2.5f, 0, 0));
            }
            
        }else{
            if (obj.GetComponent<LineRenderer>() != null)
            {
                Bounds bounds = obj.GetComponent<Renderer>().bounds;
                Vector3 size = bounds.size;

                obj.GetComponent<LineRenderer>().SetPosition(0, new Vector3(0, size.y / obj.gameObject.transform.localScale.y / 2.5f, 0));
                obj.GetComponent<LineRenderer>().SetPosition(1, new Vector3(0, -1 * size.y / obj.gameObject.transform.localScale.y / 2.5f, 0));
            }
        }

        return orientation;
    }
    
    public static void PrimaryButtonPressed(GameManager manager, OrientationBlockConfig config)
    {
        if (isTrialRunning)
        {
            StopOrientationTrial(manager, LineOrientation.Vertical, config);
        }
    }

    public static void SecondaryButtonPressed(GameManager manager, OrientationBlockConfig config)
    {
        if (isTrialRunning)
        {
            StopOrientationTrial(manager,  LineOrientation.Horizontal, config);
        }
    }

    public static void BlockEnd(GameManager manager, OrientationBlockConfig config)
    {
        // Save the data to a file
        string json = JsonUtility.ToJson(OrientationTrials.trials);
        System.IO.File.WriteAllText("orientation_trials_"+ OrientationTrials.trials.participantID + "_"+ OrientationTrials.trials.blockID.ToString("00") + ".json", json);
        OrientationTrials.trials = new();
        
        // Format: orientation_trials_XXXX_YY.json where XXXX is the participant ID and YY is the block ID


        Vector3 cameraPos = vrCamera.transform.position;
        Vector3 cameraForward = vrCamera.transform.forward * config.distanceFromUserMeters;
        Vector3 shapeCenter = cameraPos + cameraForward;

        Instantiate(manager.checkmark, shapeCenter,Quaternion.identity).transform.SetParent(trialObjectsParent.transform);
        
        manager.StartCoroutine(manager.ClearTrialObjects());

        manager.isStartLockedOut = false;
        manager.trial = Trial.NoTrial;
        manager.isTrialRunning = false;
    }
}
