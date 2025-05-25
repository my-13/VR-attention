using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using Unity.VRTemplate;
using UnityEditor;
using UnityEngine;


[Serializable]
public class OrientationBlockData
{
    //public OrientationBlockConfig saveDataUsed;
    
    public string participantID = "0000";

    public int blockID = 0;
    public List<float> trialTimesMiliseconds = new();
    public List<LineOrientation> selectedOrientation = new();

    public List<LineOrientation> actualOrientation = new();
    public List<Vector3> itemPositions = new();
    public List<bool> hadDistractor = new();
}

public enum LineOrientation
{
    Horizontal,
    Vertical
}

public enum FeedbackType
{
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
    public static (Queue<long>, Queue<Pose>, Queue<Pose>) viewTrialData = (new(), new(), new());
    public static Vector3 distractorObjPosition = Vector3.zero;
    public static Vector3 targetObjPosition = Vector3.zero;



    /**
        Generates static data for each block
    */
    public static void BlockStart(GameManager manager, ProcedureConfig config)
    {

        if (manager.isStartLockedOut)
        {
            return;
        }
        // Global Stopwatch used for timing
        stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        // Lock out the start button, until the block is over, and setting global data
        manager.isStartLockedOut = true;
        manager.trial = Trial.Orientation;
        trials = new OrientationBlockData();

        if (manager.participantID == "0000"){
            trials.participantID = GetUnusedParticipantID();
        }else {
            trials.participantID = manager.participantID;
        }

        // Saving the GameObjects for the trial to be used. 
        // TODO: replace all times that I use vrCamera with the manager.vrCamera
        vrCamera = manager.vrCamera;
        trialObjectsParent = manager.trialObjectsParent;

        // Show block instructions to the participant
        ShowUI(manager, config);
    }

    public static string GetUnusedParticipantID()
    {
        int numID = 0;
        string[] files = Directory.GetFiles("./data/");
        // Loop through all the folder names in the data folder, and find the highest number
        // They will always be in the format of "0000" or "0001" or "0002"
        foreach (string file in files)
        {
            string[] split = file.Split('/');
            string folderName = split[split.Length - 1];
            if (folderName.Length == 4 && int.TryParse(folderName, out int id))
            {
                numID = Math.Max(numID, id);
            }
        }

        // Increment the number by 1, and return it as a string with leading zeros
        numID++;
        string participantID = numID.ToString("0000");
        return participantID;
    }

    /** 
        Show the block instructions for the particpant.
        This requires the Right controller trigger to be pressed to start the trial
    */
    public static void ShowUI(GameManager manager, ProcedureConfig config)
    {

        // Grab the UI text object, rewrite text for the block instructions, and then show text
        GameObject[] uiTextArr = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(sr => sr.name == "StartText").ToArray();
        GameObject uiText = uiTextArr[0];

        manager.isUIShown = true;
        if (config.GetCurrentFeedbackType() == FeedbackType.ButtonInput)
        {
            uiText.GetComponent<TMPro.TextMeshProUGUI>().fontSize = 0.15f;
            uiText.GetComponent<TMPro.TextMeshProUGUI>().text = "In this next section, you will be asked to identify the orientation of the line for the octahedron as quickly as possible. \n If the line is vertical, press the top button on the the controller (A/X).\n If the line is horizontal, press the bottom button (B/Y). \n Press the right controller trigger button to start.";
        }
        else if (config.GetCurrentFeedbackType() == FeedbackType.Reaching)
        {
            uiText.GetComponent<TMPro.TextMeshProUGUI>().fontSize = 0.15f;
            uiText.GetComponent<TMPro.TextMeshProUGUI>().text = "In this next section, you will be asked to find the octahedron as quickly as possible, shown below. \n Once you have found the object below, reach out and grab using the controller, with your dominant hand. \nThe object may change color, or may stay the same color through this section.\nAs soon as you grab the object, it will dissapear, and the trial will repeat. Please keep your arm up, centered in between the objects if possible. \nPress the right trigger button to start.";
        }
        uiText.SetActive(true);
    }


    /** 
        Setup the block for the participant
    */
    public static void SetupBlock(GameManager manager, ProcedureConfig config)
    {

        // All that is Coded is to change the wall color to the background color
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        foreach (GameObject wall in walls)
        {
            wall.GetComponent<Renderer>().material.color = config.GetOrientationBlockConfig().backgroundColor;
        }
    }


    /**
        Waits for a random amount of time (seconds) between the pre-defined ranges before starting the trial
    */
    public static IEnumerator WaitForTrial(GameManager manager, bool wait, ProcedureConfig config)
    {

        if (wait)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(config.GetOrientationBlockConfig().timeToSpawnMin, config.GetOrientationBlockConfig().timeToSpawnMax));
            RunOrientationTrial(manager, config);
        }
        yield return new WaitForSeconds(0);
        RunOrientationTrial(manager, config);
    }

    public static IEnumerator TimeoutBreak(GameManager manager, int waitTime, ProcedureConfig config)
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
        // Remove text to wait

    }
    public static void RunOrientationTrial(GameManager manager, ProcedureConfig config)
    {
        if (isTrialRunning == true)
        {
            // Wait until the trial is over before starting a new one
            return;
        }
        string nextString = manager.configOptions.GetCurrentTrialString();
        isTrialRunning = true;
        manager.isTrialRunning = true;


        // Get the camera position and forward vector
        Vector3 cameraPos = manager.vrCamera.transform.position;
        Vector3 spawnPos = GameObject.FindGameObjectsWithTag("ItemSpawn")[0].transform.position;

        // Calculate the vector from the camera to the center of the spawn point
        Vector3 cameraForward = (spawnPos - cameraPos).normalized * config.GetOrientationBlockConfig().distanceFromUserMeters;
        Vector3 shapeCenter = cameraPos + cameraForward;

        (Vector3[], GameObject[], int) pointsData = manager.GenerateShapePoints(config.GetOrientationBlockConfig());
        // Extracting points Data
        Vector3[] objectPositions = pointsData.Item1;
        GameObject[] trialObjects = pointsData.Item2;
        int numberOfObjects = pointsData.Item3;

        // TODO: Save these in the trial data. 

        trials.itemPositions = objectPositions.Select(pos => pos + shapeCenter).ToList();

        // Shuffle the positions of the objects to randomize study
        var rng = new System.Random();
        GameManager.Shuffle(rng, objectPositions);


        // Get the starting time
        start_time_ms = stopwatch.ElapsedMilliseconds;


        // Start capturing data
        mainTrialData = (new(), new(), new(), new(), new(), new(), new());
        viewTrialData = (new(), new(), new());
        isDataRecording = true;

        Color normalColor = config.GetOrientationBlockConfig().regularItemColor;
        Color distractorColor = config.GetOrientationBlockConfig().distracterItemColor;


        if (false && config.GetOrientationBlockConfig().randomizeColors)
        {
            Color[] colors = { normalColor, distractorColor };
            GameManager.Shuffle(rng, colors);
            normalColor = colors[0];
            distractorColor = colors[1];
        }

        if (manager.configOptions.GetTrialMainColor(manager.configOptions.GetCurrentBlockNumber(), manager.configOptions.GetCurrentTrialNumber()))
        {
            Color[] colors = { normalColor, distractorColor };
            normalColor = colors[0];
            distractorColor = colors[1];
        }
        else
        {
            Color[] colors = { normalColor, distractorColor };
            normalColor = colors[1];
            distractorColor = colors[0];
        }

        // Summon The Objects
        for (int i = 0; i < objectPositions.Length; i++)
        {
            GameObject prefabObj = trialObjects[i];
            GameObject obj = Instantiate(prefabObj, objectPositions[i] + shapeCenter, prefabObj.transform.rotation).gameObject;
            obj.transform.SetParent(trialObjectsParent.transform);
            obj.transform.localScale *= config.GetOrientationBlockConfig().itemsScale;

            int currentBlock = manager.configOptions.GetCurrentBlockNumber();
            int currentTrial = manager.configOptions.GetCurrentTrialNumber();

            bool distractor = manager.configOptions.GetTrialDistractor(currentBlock, currentTrial);
            FeedbackType feedbackType = manager.configOptions.GetCurrentFeedbackType();

            if (i == 0)
            {
                obj.GetComponent<Renderer>().material.color = normalColor;
                if (feedbackType == FeedbackType.Reaching)
                {
                    // Adding the ability to grab the object
                    obj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>().enabled = true;
                    obj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>().selectEntered.AddListener((interactor) => ObjectGrabbed(manager, config, obj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>()));
                }

                if (feedbackType == FeedbackType.ButtonInput)
                {
                    itemOrientation = RandLineOrientation(obj, manager.verticalMaterial, manager.horizontalMaterial, manager.configOptions.GetCurrentOrientation());
                }
                targetObjPosition = obj.transform.position;
            }
            else if (i < config.GetOrientationBlockConfig().distractorObjects.Length + 1)
            {


                if (distractor)
                {
                    trialHadDistractor = true;
                    distractorObjPosition = obj.transform.position;
                    obj.GetComponent<Renderer>().material.color = distractorColor;
                    if (feedbackType == FeedbackType.ButtonInput)
                    {
                        RandLineOrientation(obj, manager.verticalMaterial, manager.horizontalMaterial);
                    }

                }
                else
                {
                    trialHadDistractor = false;
                    distractorObjPosition = Vector3.zero;
                    obj.GetComponent<Renderer>().material.color = normalColor;
                    if (feedbackType == FeedbackType.ButtonInput)
                    {
                        RandLineOrientation(obj, manager.verticalMaterial, manager.horizontalMaterial);
                    }
                }

            }
            else
            {
                obj.GetComponent<Renderer>().material.color = normalColor;

                if (feedbackType == FeedbackType.ButtonInput)
                {
                    RandLineOrientation(obj, manager.verticalMaterial, manager.horizontalMaterial);
                }
            }
        }

    }


    public static void RecordTrialData(GameManager manager)
    {

        // Format for text main file:
        // time, eventCode, Hand Position
        // Ex. 000001, eventCode, (0,0,0)

        // Format for text view file:
        // Time, Eye Position, Eye Rotation?
        // Ex. 000001, (0, 0, 0), (0,0,0)

        // Year, Month, Day, Hour(24), Minute, Second
        if (isDataRecording) return;
        // Make sure that data/participantID/ is created
        if (!Directory.Exists("./data"))
        {
            Directory.CreateDirectory("./data");
        }
        if (!Directory.Exists("./data/" + OrientationTrials.trials.participantID))
        {
            Directory.CreateDirectory("./data/" + OrientationTrials.trials.participantID);
        }


        string dateTime = DateTime.Now.ToString("yyyyMMddHHmmss");

        int trialID = OrientationTrials.gameManager.configOptions.GetCurrentTrialNumber();
        int blockID = manager.configOptions.GetCurrentBlockNumber();

        // I don't wanna talk about it... (actually hour 8 of development)

        int feedbackType = manager.configOptions.GetTrialFeedbackType(blockID, trialID) == FeedbackType.ButtonInput ? 1 : 0;
        int distractorPresent = manager.configOptions.GetTrialDistractor(blockID, trialID) ? 1 : 0;
        int colorVariability = manager.configOptions.GetTrialMainColor(blockID, trialID) ? 1 : 0;
        int objectType = manager.configOptions.GetOrientationBlockConfig().isItemsRealistic ? 1 : 0;


        // feedbackType ^ (2^0) + distractorPresent ^ (2^1) + colorVariability ^ (2^2) + objectType ^ (2^3)
        int category = 1 + feedbackType * 1 + distractorPresent * 2 + colorVariability * 4 + objectType * 8;

        string participantID = OrientationTrials.trials.participantID;

        // Main File (Time, EventCode, Left Hand Position, Left Hand Rotation, Right Hand position, Right Hand Rotation, Head Position, Head Rotation, Filtered Eye Data)
        string mainPath = "./data/" + participantID + "/main_" + dateTime + "_" + category + "_" + participantID + "_" + blockID + "_" + trialID + ".txt";
        string mainTrialcontent = "";

        string trialInfoPath = "./data/" + participantID + "/trial_" + "_" + category + "_" + participantID + "_" + blockID + ".txt";
        string trialInfoContent = "";


        if (manager.configOptions.GetTrialFeedbackType(blockID, trialID) == FeedbackType.Reaching)
        {
            trialInfoContent = "" +
                OrientationTrials.trials.trialTimesMiliseconds + "," +
                OrientationTrials.targetObjPosition.x + "," +
                OrientationTrials.targetObjPosition.y + "," +
                OrientationTrials.targetObjPosition.z + "," +
                OrientationTrials.distractorObjPosition.x + "," +
                OrientationTrials.distractorObjPosition.y + "," +
                OrientationTrials.distractorObjPosition.z + "," +
                OrientationTrials.trials.hadDistractor[trialID] + "\n";
        }
        else if (manager.configOptions.GetTrialFeedbackType(blockID, trialID) == FeedbackType.ButtonInput)
        {
            // Test out if feedback type is correct
            trialInfoContent = "" +
                OrientationTrials.trials.trialTimesMiliseconds + "," +
                OrientationTrials.targetObjPosition.x + "," +
                OrientationTrials.targetObjPosition.y + "," +
                OrientationTrials.targetObjPosition.z + "," +
                OrientationTrials.distractorObjPosition.x + "," +
                OrientationTrials.distractorObjPosition.y + "," +
                OrientationTrials.distractorObjPosition.z + "," +
                OrientationTrials.trials.hadDistractor[trialID] + "," +

                OrientationTrials.trials.selectedOrientation[trialID] + "," +
                OrientationTrials.trials.actualOrientation[trialID] + "\n";
        }

        // Raw Eye Data File (Time, Raw Eye Position, Raw Eye Rotation)
        string eyePath = "./data/" + OrientationTrials.trials.participantID +  "/eye_" + dateTime + "_" + category + "_" + participantID + "_" + blockID + "_" + trialID + ".txt";
        string eyeTrialContent = "";


        // Summary File (Orientation, Selected, Actual, Had Distractor)
        // Write this into a summary file

        for (int i = 0; i < mainTrialData.Item1.Count; i++)
        {
            mainTrialcontent += mainTrialData.Item1[i] + ", " + mainTrialData.Item2[i] + ", " + mainTrialData.Item3[i].x + "," + mainTrialData.Item3[i].y + "," + mainTrialData.Item3[i].z + "," + mainTrialData.Item4[i].x + "," + mainTrialData.Item4[i].y + "," + mainTrialData.Item4[i].z + "," + mainTrialData.Item5.Peek().position.x + "," + mainTrialData.Item5.Peek().position.y + "," + mainTrialData.Item5.Peek().position.z + "," + mainTrialData.Item6[i].x + "," + mainTrialData.Item6[i].y + "," + mainTrialData.Item6[i].z + "," + mainTrialData.Item7[i].x + "," + mainTrialData.Item7[i].y + "," + mainTrialData.Item7[i].z + "\n";
        }

        while(viewTrialData.Item1.Count > 0)
        {
            Pose leftEyeGaze = viewTrialData.Item2.Dequeue();
            Pose rightEyeGaze = viewTrialData.Item3.Dequeue();
            // Pop the data from the queue and write it to the file
            eyeTrialContent += viewTrialData.Item1.Dequeue().ToString() + ", " + leftEyeGaze.position.ToString() + ", " + leftEyeGaze.rotation.ToString() + ", " + rightEyeGaze.position.ToString() + ", " + rightEyeGaze.rotation.ToString() + "\n";
        }

        if (!File.Exists("./data"))
        {
            Directory.CreateDirectory("./data");
        }

        // Writing General Trial Data on if it's right or wrong
        if (!File.Exists(trialInfoPath))
        {
            File.WriteAllText(trialInfoPath, trialInfoContent);
        }
        else
        {
            File.AppendAllText(trialInfoPath, trialInfoContent);
        }

        // Writing Eye Tracking Data
        if (!File.Exists(eyePath))
        {
            File.WriteAllText(eyePath, eyeTrialContent);
        }
        else
        {
            File.AppendAllText(eyePath, eyeTrialContent);
        }

        // Writing Main Trial Tracking Data
        if (!File.Exists(mainPath))
        {
            File.WriteAllText(mainPath, mainTrialcontent);
        }
        else
        {
            File.AppendAllText(mainPath, mainTrialcontent);
        }

    }

    public static IEnumerator StopRecordingDataDelay(float delay, GameManager manager, ProcedureConfig config)
    {
        // Destroy all objects
        foreach (Transform transform in trialObjectsParent.transform)
        {
            Destroy(transform.gameObject);
        }

        yield return new WaitForSeconds(delay);
        isDataRecording = false;
        RecordTrialData(manager);


        manager.configOptions.SetNextTrial();


        if (manager.configOptions.IsLastTrial())
        {

            if (manager.configOptions.IsLastBlock())
            {

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
            manager.StartCoroutine(WaitForTrial(manager, true, config));
        }
    }

    public static void StopOrientationTrial(GameManager manager, ProcedureConfig config)
    {
        // Get the ending time
        long end_time_ms = stopwatch.ElapsedMilliseconds;
        // Calculate the time taken
        time_ms = end_time_ms - start_time_ms;

        // Print the time taken
        Debug.Log("Time taken: " + time_ms + "ms");

        isTrialRunning = false;
        manager.isTrialRunning = false;

        trials.trialTimesMiliseconds.Add(time_ms);
        trials.hadDistractor.Add(trialHadDistractor);
        manager.StartCoroutine(StopRecordingDataDelay(0.5f, manager, config));
    }

    public static void StopOrientationTrial(GameManager manager, LineOrientation orientation, ProcedureConfig config)
    {
        bool wasCorrect = orientation == itemOrientation;
        trials.actualOrientation.Add(itemOrientation);
        trials.selectedOrientation.Add(orientation);

        StopOrientationTrial(manager, config);
    }

    public static LineOrientation RandLineOrientation(GameObject obj, Material verticalMaterial, Material horizontalMaterial)
    {
        LineOrientation orientation = (LineOrientation)UnityEngine.Random.Range(0, 2);
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

        }
        else
        {
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

        }
        else
        {
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

    public static void UIStartPressed(GameManager manager, ProcedureConfig config)
    {
        GameObject[] uiTextArr = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(sr => sr.name == "StartText").ToArray();
        GameObject uiText = uiTextArr[0];
        uiText.SetActive(false);
        manager.isUIShown = false;

        SetupBlock(manager, config);
        manager.StartCoroutine(WaitForTrial(manager, true, config));
    }

    public static void ObjectGrabbed(GameManager manager, ProcedureConfig config, UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable)
    {
        if (interactable.isSelected)
        {
            List<UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor> interactor = interactable.interactorsSelecting;
            if (interactor[0] != null)
            {
                UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable heldObject = interactor[0].interactablesSelected[0];
                if (heldObject != null)
                {
                    Destroy(heldObject.transform.gameObject);
                }

            }
        }
        if (isTrialRunning)
        {
            StopOrientationTrial(manager, config);

        }
    }

    public static void PrimaryButtonPressed(GameManager manager, ProcedureConfig config)
    {
        if (isTrialRunning)
        {
            StopOrientationTrial(manager, LineOrientation.Vertical, config);
        }
    }

    public static void SecondaryButtonPressed(GameManager manager, ProcedureConfig config)
    {
        if (isTrialRunning)
        {
            StopOrientationTrial(manager, LineOrientation.Horizontal, config);
        }
    }

    public static void BlockEnd(GameManager manager, ProcedureConfig config)
    {
        // Save the data to a file
        string json = JsonUtility.ToJson(OrientationTrials.trials);
        string dateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
        string dataPath = "./data/" + OrientationTrials.trials.participantID + "/data_" + dateTime + "_" + OrientationTrials.trials.participantID + "_" + OrientationTrials.trials.blockID.ToString("00") + ".json";


        // Make sure the path exists
        EnsureDataPath("./data/");
        EnsureDataPath("./data/" + OrientationTrials.trials.participantID + "/");

        // Write the data to a file
        if (!File.Exists(dataPath))
        {
            File.WriteAllText(dataPath, json);
        }
        else
        {
            File.AppendAllText(dataPath, json);
        }

        // Set the next trial in the config a
        manager.configOptions.SetNextTrial();

        // Format: orientation_trials_XXXX_YY.json where XXXX is the participant ID and YY is the block ID
        Vector3 cameraPos = vrCamera.transform.position;
        Vector3 cameraForward = vrCamera.transform.forward * config.GetOrientationBlockConfig().distanceFromUserMeters;
        Vector3 shapeCenter = cameraPos + cameraForward;

        Instantiate(manager.checkmark, shapeCenter, Quaternion.identity).transform.SetParent(trialObjectsParent.transform);

        manager.StartCoroutine(manager.ClearTrialObjects());

        manager.isStartLockedOut = false;
        manager.trial = Trial.NoTrial;
        manager.isTrialRunning = false;
    }

    public static void EnsureDataPath(string path)
    {
        if (!File.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}
