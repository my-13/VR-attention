using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using Unity.VRTemplate;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[Serializable]
public class OrientationBlockData
{
    //public OrientationBlockConfig saveDataUsed;
    public string participantID = "0000";
    public int blockID = 0;
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

public enum FeedbackType{
    Reaching,
    ButtonInput
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
    public static long start_time_ms = 0;
    private static LineOrientation itemOrientation;
    [HideInInspector]
    public static OrientationBlockData trials;
    public static bool isTrialRunning = false;
    public static bool isDataRecording = false;
    private static bool trialHadDistractor = false;
    [HideInInspector]
    public static GameObject trialObjectsParent;
    //[HideInInspector]
    //public static ConfigOptions configOptions;
    [HideInInspector]
    public static Camera vrCamera;
    
    
    //private static float radius;
    private static GameObject[] objects;
    public static GameManager gameManager;

    // (Miliseconds, EventCode, Left Hand, Right Hand, Eye Queue, Head Position, Head Rotation) 50Hz
    public static (List<long>, List<TrialEvent>, List<Vector3>, List<Vector3>, Queue<Pose>, List<Vector3>, List<Quaternion>) mainTrialData = (new(), new(), new(), new(), new(), new(), new());
    // (Miliseconds, Eye Pose at that time) 120Hz
    public static (List<long>, Queue<Pose>) viewTrialData = (new(), new());
    public static Vector3 distractorObjPosition = Vector3.zero;
    public static Vector3 targetObjPosition = Vector3.zero;




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
        ShowUI(manager, config);
    }

    public static void ShowUI(GameManager manager, OrientationBlockConfig config)
    {

        
        GameObject[] uiTextArr = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(sr => sr.name == "StartText").ToArray();
        GameObject uiText = uiTextArr[0];
        manager.isUIShown = true;
        uiText.SetActive(true);
        if (config.feedbackType == FeedbackType.ButtonInput){
            uiText.GetComponent<TMPro.TextMeshProUGUI>().fontSize = 0.15f;
            uiText.GetComponent<TMPro.TextMeshProUGUI>().text = "In this next section, you will be asked to identify the orientation of the line as quickly as possible. \n If the line is horizontal, press the top button on the the controller (A/X).\n If the line is vertical, press the bottom button (B/Y). \n Press the right controller trigger button to start.";
        }else if(config.feedbackType == FeedbackType.Reaching){
            uiText.GetComponent<TMPro.TextMeshProUGUI>().fontSize = 0.15f;
            uiText.GetComponent<TMPro.TextMeshProUGUI>().text = "In this next section, you will be asked to find an object as quickly as possible, shown below. \n Once you have found the object below, reach out and grab using the controller, with your dominant hand. \nThe object may change color, or may stay the same color through this section.\nAs soon as you grab the object, it will dissapear, and the trial will repeat. \nPress the right trigger button to start.";
        }

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
        uiText.GetComponent<TMPro.TextMeshProUGUI>().text = "Please take a break for " + waitTime + " seconds. Press the left controller trigger button after screen clears.";
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
        mainTrialData = (new(), new(), new(), new(), new(), new(), new());
        viewTrialData = (new(), new());
        isDataRecording = true;

        Color normalColor = config.regularItemColor;
        Color distractorColor = config.distracterItemColor;


        if (false && config.randomizeColors)
        {
            Color[] colors = {normalColor, distractorColor};
            GameManager.Shuffle(rng, colors);
            normalColor = colors[0];
            distractorColor = colors[1];
        }

        if (manager.configOptions.procedureConfig.GetCurrentMainColor()){
            Color[] colors = {normalColor, distractorColor};
            normalColor = colors[0];
            distractorColor = colors[1];
        }else{
            Color[] colors = {normalColor, distractorColor};
            normalColor = colors[1];
            distractorColor = colors[0];
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
                if (manager.configOptions.procedureConfig.GetCurrentFeedbackType() == FeedbackType.Reaching)
                {
                    obj.GetComponent<XRGrabInteractable>().enabled = true;
                    obj.GetComponent<XRGrabInteractable>().selectEntered.AddListener((interactor) => ObjectGrabbed(manager, config, obj.GetComponent<XRGrabInteractable>() ));
                }
                
                if (manager.configOptions.procedureConfig.GetCurrentFeedbackType() == FeedbackType.ButtonInput)
                {
                    itemOrientation = RandLineOrientation(obj, manager.verticalMaterial, manager.horizontalMaterial,  manager.configOptions.procedureConfig.GetCurrentOrientation());
                }
                targetObjPosition = obj.transform.position;
            }
            else if (i < config.distractorObjects.Length + 1)
            {
                
                if (manager.configOptions.procedureConfig.GetCurrentDistractor() )
                {
                    trialHadDistractor = true;
                    distractorObjPosition = obj.transform.position;
                    obj.GetComponent<Renderer>().material.color = distractorColor;
                    if (manager.configOptions.procedureConfig.GetCurrentFeedbackType() == FeedbackType.ButtonInput)
                    {
                        RandLineOrientation(obj, manager.verticalMaterial, manager.horizontalMaterial);
                    }
                    
                }
                else
                {
                    trialHadDistractor = false;
                    distractorObjPosition = Vector3.zero;
                    obj.GetComponent<Renderer>().material.color = normalColor;
                    if (manager.configOptions.procedureConfig.GetCurrentFeedbackType() == FeedbackType.ButtonInput)
                    {
                        RandLineOrientation(obj, manager.verticalMaterial, manager.horizontalMaterial);
                    }
                }

            }
            else
            {
                obj.GetComponent<Renderer>().material.color = normalColor;

                if (manager.configOptions.procedureConfig.GetCurrentFeedbackType() == FeedbackType.ButtonInput)
                {
                    RandLineOrientation(obj, manager.verticalMaterial, manager.horizontalMaterial);
                }
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
        
        // Year, Month, Day, Hour(24), Minute, Second
        if (isDataRecording) return;
        
        string dateTime = DateTime.Now.ToString("yyyyMMddHHmmss");

        int feedbackType = manager.configOptions.procedureConfig.GetCurrentFeedbackType() == FeedbackType.ButtonInput ? 1 : 0;
        int distractorPresent = manager.configOptions.procedureConfig.GetCurrentDistractor() ? 1 : 0;
        int colorVariability = manager.configOptions.GetCurrentBlockConfig().isMainColorSwapped ? 1 : 0;
        int objectType = manager.configOptions.GetCurrentBlockConfig().isItemsRealistic ? 1 : 0;

        
        // feedbackType ^ (2^0) + distractorPresent ^ (2^1) + colorVariability ^ (2^2) + objectType ^ (2^3)
        int category = 1 + feedbackType * 1 + distractorPresent * 2 + colorVariability * 4 + objectType * 8;
        
        string participantID = OrientationTrials.trials.participantID;
        int currentTrialID = OrientationTrials.gameManager.configOptions.procedureConfig.currentTrial;
        int currentBlockID = manager.configOptions.procedureConfig.currentBlock;

        // Main File (Time, EventCode, Left Hand Position, Left Hand Rotation, Right Hand position, Right Hand Rotation, Head Position, Head Rotation, Filtered Eye Data)
        string mainPath = "./data/main_" + dateTime + "_" + category + "_" + participantID + "_" + currentBlockID + "_" + currentTrialID + ".txt";
        string mainTrialcontent = "";

        string trialInfoPath = "./data/trial_" + "_" + category + "_" + participantID + "_" + currentBlockID + ".txt";
        string trialInfoContent = "";
        
        Debug.Log(currentTrialID);
        if (manager.configOptions.procedureConfig.GetCurrentFeedbackType() == FeedbackType.Reaching){
            trialInfoContent = "" + OrientationTrials.trials.hadDistractor[currentTrialID] + "," + OrientationTrials.trials.trialTimesMiliseconds + "," + OrientationTrials.targetObjPosition.x + "," + OrientationTrials.targetObjPosition.y + ","+ OrientationTrials.targetObjPosition.z + "," + OrientationTrials.distractorObjPosition.x + "," + OrientationTrials.distractorObjPosition.y + "," + OrientationTrials.distractorObjPosition.z + "\n";
        }
        else if(manager.configOptions.procedureConfig.GetCurrentFeedbackType() == FeedbackType.ButtonInput){
            trialInfoContent = "" +  OrientationTrials.trials.selectedOrientation[currentTrialID] + "," + OrientationTrials.trials.actualOrientation[currentTrialID] +"," +OrientationTrials.trials.hadDistractor[currentTrialID] + OrientationTrials.trials.trialTimesMiliseconds[currentTrialID]+  ","+ OrientationTrials.targetObjPosition.x + "," + OrientationTrials.targetObjPosition.y + ","+ OrientationTrials.targetObjPosition.z + "," + OrientationTrials.distractorObjPosition.x + "," + OrientationTrials.distractorObjPosition.y + "," + OrientationTrials.distractorObjPosition.z + "\n";
        }
        //trialTimesMiliseconds
        

        // Raw Eye Data File (Time, Raw Eye Position, Raw Eye Rotation)
        string eyePath = "./data/eye_" + dateTime + "_" + category + "_" + participantID + "_" + currentBlockID + "_" + currentTrialID + ".txt";
        string eyeTrialContent = "";


        // Summary File (Orientation, Selected, Actual, Had Distractor)
        int currentTrialNumber = OrientationTrials.gameManager.configOptions.procedureConfig.currentTrial - 1;
        
        int selectedOrientation = (int)OrientationTrials.trials.selectedOrientation[currentTrialNumber ];
        int actualOrientation = (int)OrientationTrials.trials.actualOrientation[currentTrialNumber ];
        int hadDistractor = OrientationTrials.trials.hadDistractor[currentTrialNumber ] ? 1 : 0;
        string mainTrialInfo = selectedOrientation + ", " + actualOrientation + ", " + hadDistractor + "\n";
        // Write this into a summary file

        for (int i = 0; i < mainTrialData.Item1.Count; i++)
        {
            mainTrialcontent += mainTrialData.Item1[i] + ", " + mainTrialData.Item2[i] + ", " + mainTrialData.Item3[i].x + "," + mainTrialData.Item3[i].y + "," + mainTrialData.Item3[i].z + "," + mainTrialData.Item4[i].x + "," + mainTrialData.Item4[i].y + "," + mainTrialData.Item4[i].z + "," + mainTrialData.Item5.Peek().position.x + "," + mainTrialData.Item5.Peek().position.y + "," + mainTrialData.Item5.Peek().position.z + "," + mainTrialData.Item6[i].x + "," + mainTrialData.Item6[i].y + "," + mainTrialData.Item6[i].z + ","+ mainTrialData.Item7[i].x + "," + mainTrialData.Item7[i].y + "," + mainTrialData.Item7[i].z +"\n";
        }

        for (int i = 0; i < viewTrialData.Item1.Count; i++)
        {
            eyeTrialContent += viewTrialData.Item1[i] + ", " + viewTrialData.Item2.Peek().position + ", " + viewTrialData.Item2.Peek().rotation + "\n";
        }

        if (!File.Exists("./data")) {
            Directory.CreateDirectory("./data");
        }

        // Writing General Trial Data on if it's right or wrong
        if (!File.Exists(trialInfoPath)) {
            File.WriteAllText(trialInfoPath, trialInfoContent);
        }else{
            File.AppendAllText(trialInfoPath, trialInfoContent);
        }

        // Writing Eye Tracking Data
        if (!File.Exists(eyePath)) {
            File.WriteAllText(eyePath, eyeTrialContent);
        }else{
            File.AppendAllText(eyePath, eyeTrialContent);
        }

        // Writing Main Trial Tracking Data
        if (!File.Exists(mainPath)) {
            File.WriteAllText(mainPath, mainTrialcontent);
        }else{
            File.AppendAllText(mainPath, mainTrialcontent);    
        }

    }
    
    public static IEnumerator StopRecordingDataDelay(float delay, GameManager manager){
        yield return new WaitForSeconds(delay);
        isDataRecording = false;
        RecordTrialData(manager);
    }

    public static void StopOrientationTrial(GameManager manager, OrientationBlockConfig config)
    {   
        // Get the ending time
        long end_time_ms = stopwatch.ElapsedMilliseconds;
        // Calculate the time taken
        time_ms = end_time_ms - start_time_ms;

        // Print the time taken
        Debug.Log("Time taken: " + time_ms + "ms");
        
        isTrialRunning = false;
        manager.isTrialRunning = false;
        manager.StartCoroutine(StopRecordingDataDelay(0.5f, manager));
        

        trials.trialTimesMiliseconds.Add(time_ms);
        trials.hadDistractor.Add(trialHadDistractor);
        

        // Destroy all objects
        foreach (Transform transform in trialObjectsParent.transform)
        {
            Destroy(transform.gameObject);
        }
        string trialString = manager.configOptions.procedureConfig.GetNextTrialString();

        if (manager.configOptions.procedureConfig.IsOverTrial())
        {
            
            if (manager.configOptions.procedureConfig.IsLastBlock()){
                
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
        manager.StartCoroutine(StopRecordingDataDelay(1f, manager));
        


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
        
        string trialString = manager.configOptions.procedureConfig.GetNextTrialString();

        if (manager.configOptions.procedureConfig.currentTrial >= config.numberOfTrials)
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
        // Check to see if it's a grab trial, if so, then just don't generate the lines.

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

    public static LineOrientation RandLineOrientation(GameObject obj, Material verticalMaterial, Material horizontalMaterial, LineOrientation orientation)
    {
        // Check to see if it's a grab trial, if so, then just don't generate the lines.

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
    
    public static void UIStartPressed(GameManager manager, OrientationBlockConfig config)
    {
        GameObject[] uiTextArr = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(sr => sr.name == "StartText").ToArray();
        GameObject uiText = uiTextArr[0];
        uiText.SetActive(false);
        manager.isUIShown = false;

        SetupBlock(manager, config);
        manager.StartCoroutine(WaitForTrial(manager, true, config));
    }

    public static void ObjectGrabbed(GameManager manager, OrientationBlockConfig config, XRGrabInteractable interactable)
    {   
        if (interactable.isSelected){
            List<IXRSelectInteractor> interactor = interactable.interactorsSelecting;
            if (interactor[0] != null){
                IXRSelectInteractable heldObject = interactor[0].interactablesSelected[0];
                if (heldObject != null){
                    Destroy(heldObject.transform.gameObject);
                }
                
            }
        }
        if (isTrialRunning)
        {
            StopOrientationTrial(manager, config);
        
        }
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
        string dateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
        string dataPath = "./data/data_"+ dateTime + "_" + OrientationTrials.trials.participantID + "_" + OrientationTrials.trials.blockID.ToString("00") + ".json";

        if (!File.Exists(dataPath)) {
            File.WriteAllText(dataPath, json);
        }else{
            File.AppendAllText(dataPath, json);
        }
        
        OrientationTrials.trials = new();
        manager.configOptions.procedureConfig.currentTrial = 0;
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
