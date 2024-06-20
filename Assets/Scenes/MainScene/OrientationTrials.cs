using System;
using System.Collections;
using System.Collections.Generic;
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
    public int numberOfTrials = 0; // This should be equivalent to the number of trialTimes
    public List<float> trialTimesMiliseconds = new();
    public List<LineOrientation> selectedOrientation = new();

    public List<LineOrientation> actualOrientation = new();
    
    public List<bool> hadDistractor = new();
}

public enum LineOrientation
{
    Horizontal,
    Vertical
}

public class OrientationTrials : MonoBehaviour
{

    private static System.Diagnostics.Stopwatch stopwatch;
    private static long time_ms = 0;
    private static long start_time_ms = 0;
    private static LineOrientation itemOrientation;
    [HideInInspector]
    public static OrientationBlockData trials;
    private static bool isTrialRunning = false;
    private static bool trialHadDistractor = false;
    [HideInInspector]
    public static GameObject trialObjectsParent;
    [HideInInspector]
    public static ConfigOptions configOptions;
    [HideInInspector]
    public static Camera vrCamera;
    
    //private static float radius;
    private static GameObject[] objects;


    public static void TrialStart(GameManager manager, OrientationBlockConfig config)
    {
        stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        manager.trial = Trial.Orientation;
        trials = new OrientationBlockData();
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

        yield return new WaitForSeconds(waitTime);


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

        trials.numberOfTrials++;
        trials.trialTimesMiliseconds.Add(time_ms);
        bool wasCorrect = orientation == itemOrientation;
        trials.actualOrientation.Add(itemOrientation);
        trials.selectedOrientation.Add(orientation);
        trials.hadDistractor.Add(trialHadDistractor);

        // Destroy all objects
        foreach (Transform transform in trialObjectsParent.transform)
        {
            Destroy(transform.gameObject);
        }
        
        if (trials.numberOfTrials >= config.numberOfTrials)
        {
            BlockEnd(manager, config);
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
            obj.GetComponent<Renderer>().materials = new Material[] {obj.GetComponent<Renderer>().material, horizontalMaterial};
        }else{
            obj.GetComponent<Renderer>().materials = new Material[] {obj.GetComponent<Renderer>().material, verticalMaterial};
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
        System.IO.File.WriteAllText("orientation_trials.json", json);

        Vector3 cameraPos = vrCamera.transform.position;
        Vector3 cameraForward = vrCamera.transform.forward * config.distanceFromUserMeters;
        Vector3 shapeCenter = cameraPos + cameraForward;

        Instantiate(manager.checkmark, shapeCenter,Quaternion.identity).transform.SetParent(trialObjectsParent.transform);
        
        manager.StartCoroutine(manager.ClearTrialObjects());

        manager.EndStudy();
        manager.trial = Trial.NoTrial;
        manager.isTrialRunning = false;
    }
}
