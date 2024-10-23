using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// The different types of trials that can be run
public enum Trial 
{
    NoTrial,
    Orientation,
    ColorMemory, // [Deprecated]
}



public class GameManager : MonoBehaviour
{
    // Global Variables, used to keep track of the state of the game
    public string participantID = "0000";
    public bool isTrialRunning = false;
    private bool isStudyRunning = false;
    public bool isUIShown = false;
    public bool isStartLockedOut = false;

    public ProcedureConfig configOptions;
    public Trial trial = Trial.NoTrial;

    // Objects that are recorded for trial data. Left and right hand positions
    public GameObject rightHandObject;
    public GameObject leftHandObject;
    
    // Objects referenced to be passed to the trials scripts
    public GameObject linePrefab;
    public GameObject trialObjectsParent;
    public GameObject checkmark;
    public Material verticalMaterial;
    public Material horizontalMaterial;

    // Eye tracking thread. Runs the RecordEyeData function
    public Thread eyeThread;

    public Camera vrCamera;

    // Input actions, used to record button inputs or eye tracking data
    private InputActionAsset ActionAsset;
    public InputActionReference ViewEyeGaze;
    public InputActionReference startAction;
    public InputActionReference primaryAction;
    public InputActionReference secondaryAction;

    

    // Start is called before the first frame update
    void Start()
    {   
        // Spawning a grey sphere at the center of the screen. This is used to focus the user's attention
        GameObject focusSphere = GameObject.FindGameObjectsWithTag("ItemSpawn")[0];
        // Spawns at eye level with VR headset
        focusSphere.transform.position = new Vector3(focusSphere.transform.position.x, vrCamera.transform.position.y, focusSphere.transform.position.z);
    }



    public void StartRandomGame()
    {   
        // If the study is already running, start the next trial
        if (isStudyRunning == true)
        {
            // Checking if the start button is locked out. This is for making sure the user doesn't accidentally start a trial when inside a block
            if (isStartLockedOut == false)
            {

                if (configOptions.IsBlockAvailable())
                {
                    OrientationTrials.BlockStart(this, configOptions.GetOrientationBlockConfig());
                }
            }
            return;
        }

        // Get starting text and hide it
        GameObject[] uiText = GameObject.FindGameObjectsWithTag("StartUI");
        foreach (GameObject text in uiText)
        {
            text.SetActive(false);
        }
        

        //Show Explaining UI, explaining what steps participants should do in this block
        isUIShown = true;

        // Generating Procedure Config, read from file
        configOptions = new ProcedureConfig("procedure", "procedure.txt");
        isStudyRunning = true;
        
        // Eye Tracking Recording Thread
        eyeThread = new( new ThreadStart(() => RecordEyeData()));
        ViewEyeGaze.action.Enable();
        eyeThread.Start();

        // Start the first trial
        if (!configOptions.IsLastBlock()){
            OrientationTrials.gameManager = this;
            OrientationTrials.BlockStart(this, configOptions.GetOrientationBlockConfig());
        }
    }


    void OnApplicationQuit()
    {
        if (eyeThread != null && eyeThread.IsAlive)
        {
            eyeThread.Abort(); // Or better, use a cancellation token for safe shutdown
        }
    }


    async void RecordEyeData(){
        while(true){
            if (OrientationTrials.isDataRecording){
                Pose pose = ViewEyeGaze.action.ReadValue<Pose>();
                
                OrientationTrials.viewTrialData.Item1.Add(OrientationTrials.stopwatch.ElapsedMilliseconds - OrientationTrials.start_time_ms);
                OrientationTrials.viewTrialData.Item2.Enqueue(pose);
                await Task.Delay(8);
            }
            
        }
    }

    void FixedUpdate() {
        if (OrientationTrials.isDataRecording){
            long time_ms = OrientationTrials.stopwatch.ElapsedMilliseconds;
            // Check what is happening, see if there's any input.
            TrialEvent code = TrialEvent.Nothing;

            //InputActionAsset
            if ( startAction.action.triggered)
            {
                code = TrialEvent.TriggerPressed;
            }

            if (primaryAction.action.triggered || secondaryAction.action.triggered)
            {
                code = TrialEvent.ButtonPressed;
            }

        

            // Log the data
            OrientationTrials.mainTrialData.Item1.Add(time_ms - OrientationTrials.start_time_ms);
            OrientationTrials.mainTrialData.Item2.Add(code);
            OrientationTrials.mainTrialData.Item3.Add(rightHandObject.transform.position);
            OrientationTrials.mainTrialData.Item4.Add(leftHandObject.transform.position);
            OrientationTrials.mainTrialData.Item6.Add(vrCamera.transform.position);
            OrientationTrials.mainTrialData.Item7.Add(vrCamera.transform.rotation);
            
            // Dequeue the eye tracking data
            OrientationTrials.mainTrialData.Item5.Enqueue(OrientationTrials.viewTrialData.Item2.Count != 0 ? OrientationTrials.viewTrialData.Item2.Peek() : new Pose());
        }
    }


    public (Vector3[], GameObject[], int) GenerateShapePoints(OrientationBlockConfig blockConfig)
    {

        GameObject[] configObjects = new GameObject[blockConfig.otherObjects.Length + blockConfig.distractorObjects.Length + 1];

        configObjects[0] = blockConfig.targetObject;
        blockConfig.distractorObjects.CopyTo(configObjects, 1);
        blockConfig.otherObjects.CopyTo(configObjects, blockConfig.distractorObjects.Length + 1);



        int numPositionPoints = configObjects.Length;

        // Generating the points based on the polygon automatically, regardless of the number of points
        Vector3[] shapePositions = new Vector3[numPositionPoints];
        int phaseShiftDeg = blockConfig.randomRotation ? UnityEngine.Random.Range(0, 180) : blockConfig.rotationDegrees;
        
        // Generating the points based on the polygon automatically, regardless of the number of points
        for (int i = 0; i < numPositionPoints; i++) 
        {
            if (blockConfig.itemLocation == ItemLocation.OnTable)
            {
                shapePositions[i] = new Vector3((float)(blockConfig.radiusOfObjectsMeters * Math.Sin((phaseShiftDeg * Math.PI / 180) + (2 * Math.PI * i / numPositionPoints))), -0.5f, (float)(blockConfig.radiusOfObjectsMeters * Math.Cos((phaseShiftDeg * Math.PI / 180) + (2 * Math.PI * i / numPositionPoints))));
            }
            else if (blockConfig.itemLocation == ItemLocation.InAir)
            {
                // TODO: Implement this so that a table appears near them
                shapePositions[i] = new Vector3((float)(blockConfig.radiusOfObjectsMeters * Math.Sin((phaseShiftDeg * Math.PI / 180) + (2 * Math.PI * i / numPositionPoints))), (float)(blockConfig.radiusOfObjectsMeters * Math.Cos((phaseShiftDeg * Math.PI / 180) + (2 * Math.PI * i / numPositionPoints))), 0);
            } 
        }


        return (shapePositions, configObjects, numPositionPoints);
    }

    public void StartButtonPressed()
    {
        StartRandomGame();
    }

    public void UIStartButtonPressed()
    {
        
        if (isUIShown && trial == Trial.Orientation)
        {
            OrientationTrials.UIStartPressed(this, configOptions.GetOrientationBlockConfig());
        }
    }
    
    public void PrimaryButtonPressed()
    {
        if (isTrialRunning)
        {
            if (trial == Trial.Orientation && configOptions.GetCurrentFeedbackType() == FeedbackType.ButtonInput)
            {
                OrientationTrials.PrimaryButtonPressed(this, configOptions.GetOrientationBlockConfig());
            }
        }
    }

    public void ObjectGrabbed(){
        if (isTrialRunning)
        {
            if (configOptions.GetCurrentFeedbackType() == FeedbackType.Reaching)
            {
                //OrientationTrials.ObjectGrabbed(this, configOptions.GetCurrentBlockConfig());
            }
        }
    }

    public void SecondaryButtonPressed()
    {
        if (isTrialRunning)
        {
            if (trial == Trial.Orientation && configOptions.GetCurrentFeedbackType() == FeedbackType.ButtonInput)
            {
                OrientationTrials.SecondaryButtonPressed(this, configOptions.GetOrientationBlockConfig());
            }
        }
    }

    public void EndStudy()
    {
        Debug.Log("End of study");
        isStudyRunning = false;
    }

    public IEnumerator ClearTrialObjects()
    {
        yield return new WaitForSeconds(2);
        foreach (Transform transform in trialObjectsParent.transform)
        {
            Destroy(transform.gameObject);
        }
    }


    // Helper functions

    // Code By Matt Howels
    public static void Shuffle<T> (System.Random rng, T[] array)
    {
        int n = array.Length;
        while (n > 1) 
        {
            int k = rng.Next(n--);
            (array[k], array[n]) = (array[n], array[k]);
        }
    }

}
