using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;




[Serializable]
public class SaveData
{
    public int numberOfPoints;
    public float polygonRadius;
    public float distanceToPolygon;
    public string objectTypes;
    public int numberOfDistractions;
    public int numberOfTrials;
}

public enum Trial 
{
    NoTrial,
    Orientation,
    ColorMemory,
}



public class GameManager : MonoBehaviour
{
    // Time taken to complete the game
    public bool isTrialRunning = false;
    private bool isStudyRunning = false;
    public bool isStartLockedOut = false;
    public ConfigOptions configOptions;
    
    public Trial trial = Trial.NoTrial;
    public GameObject trackedPositionObject;
    
    // Objects referenced to be passed to the trials scripts
    public GameObject linePrefab;
    public GameObject trialObjectsParent;
    public GameObject checkmark;
    public Material verticalMaterial;
    public Material horizontalMaterial;


    // VR Camera
    public Camera vrCamera;

    // List of objects to be spawned
    
    [SerializeField] public InputActionReference ViewEyeGaze;

    [SerializeField] private InputActionAsset ActionAsset;

    private void OnEnable() 
    {
        if (ActionAsset != null) 
        {
            ActionAsset.Enable(); 

        } 
    } 

    

    // Start is called before the first frame update
    void Start()
    {
        // Set background color of walls
        // Get the walls

        GameObject focusSphere = GameObject.FindGameObjectsWithTag("ItemSpawn")[0];
        focusSphere.transform.position = new Vector3(focusSphere.transform.position.x, vrCamera.transform.position.y, focusSphere.transform.position.z);
    }



    public void StartGame()
    {
        if (isStudyRunning == true)
        {
            if (isStartLockedOut == false)
            {

                if (configOptions.IsBlockAvailable())
                {
                    // Randomize the colors for orientation
                    OrientationTrials.BlockStart(this, configOptions.GetCurrentBlockConfig());
                }
            }
            // We already have a study running, but we're not in a trial so start the next trial
            

            return;
        }

        // Get UI text
        GameObject[] uiText = GameObject.FindGameObjectsWithTag("StartUI");
        
        //Hide UI
        foreach (GameObject text in uiText)
        {
            text.SetActive(false);
        }

        isStudyRunning = true;
        
        StartCoroutine(RecordEyeData());
        //Thread eyeThread = new(RecordEyeData);
        //eyeThread.Start();

        // Start the first trial
        if (!configOptions.IsLastBlock()){
            OrientationTrials.gameManager = this;
            OrientationTrials.BlockStart(this, configOptions.GetCurrentBlockConfig());
        }
    }

    IEnumerator RecordEyeData(){
        
        while (true){
            if (OrientationTrials.isDataRecording){
                Pose pose = ViewEyeGaze.action.ReadValue<Pose>();
                OrientationTrials.viewTrialData.Item1.Add(OrientationTrials.stopwatch.ElapsedMilliseconds);
                OrientationTrials.viewTrialData.Item2.Add(pose);
            }
            
            yield return new WaitForSeconds(0.01f);
        }
        
    }

    void FixedUpdate() {
        if (OrientationTrials.isDataRecording){
            long time_ms = OrientationTrials.stopwatch.ElapsedMilliseconds;
            // Check what is happening, see if there's any input.
            TrialEvent code = TrialEvent.Nothing;

            if (new InputAction("Start").ReadValue<float>() > 0.5f)
            {
                code = TrialEvent.TriggerPressed;
            }

            if (Input.GetKeyDown("up") || Input.GetKeyDown("down"))
            {
                code = TrialEvent.ButtonPressed;
            }
        

            // Log the data
            OrientationTrials.mainTrialData.Item1.Add(time_ms);
            OrientationTrials.mainTrialData.Item2.Add(code);
            OrientationTrials.mainTrialData.Item3.Add(trackedPositionObject.transform.position);


            // Dequeue the eye tracking data
        }
    }

    void Update()
    {
        if (isTrialRunning && trial == Trial.Orientation)
        {
            // Put this in a seperate thread
            Pose pose = ViewEyeGaze.action.ReadValue<Pose>();
            OrientationTrials.trials.viewPoses.Add(pose);
            OrientationTrials.trials.viewPosesTime.Add(OrientationTrials.stopwatch.ElapsedMilliseconds);
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
        StartGame();
    }
    
    public void PrimaryButtonPressed()
    {
        if (isTrialRunning)
        {
            if (trial == Trial.Orientation)
            {
                OrientationTrials.PrimaryButtonPressed(this, configOptions.GetCurrentBlockConfig());
            }
        }
    }

    public void SecondaryButtonPressed()
    {
        if (isTrialRunning)
        {
            if (trial == Trial.Orientation)
            {
                OrientationTrials.SecondaryButtonPressed(this, configOptions.GetCurrentBlockConfig());
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

    public static void WriteLargeFile(string path, string content)
    {
        int bufferSize = 4096;
        byte[] buffer = new byte[bufferSize];

        using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.SequentialScan))
        {
            using (var writer = new StreamWriter(stream))
            {
                int offset = 0;
                while (offset < content.Length)
                {
                    int bytesToWrite = Math.Min(bufferSize, content.Length - offset);
                    var chunk = content.Substring(offset, bytesToWrite);
                    writer.Write(chunk);
                    writer.Flush();
                    offset += bytesToWrite;
                }
            }
        }
    }

    // Matt Howels
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
