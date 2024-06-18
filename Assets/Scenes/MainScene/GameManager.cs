using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;




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
    public ConfigOptions configOptions;
    public System.Diagnostics.Stopwatch stopwatch;
    
    // Objects referenced to be passed to the trials scripts
    public GameObject linePrefab;
    public GameObject trialObjectsParent;
    public GameObject checkmark;
    public Trial trial = Trial.NoTrial;

    // Polygon parameters
    private int numPoints;
    private float radius;
    private float distance;

    // List of objects to be spawned
    public GameObject[] objects;
    [HideInInspector]
    public Vector3[] shapePositions;
    public Camera vrCamera;
    public int finishedTrials = 0;


    // Start is called before the first frame update
    void Start()
    {
        // Set background color of walls
        // Get the walls
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        foreach (GameObject wall in walls)
        {
            wall.GetComponent<Renderer>().material.color = configOptions.orientationBackgroundColor;
        }


        GameObject focusSphere = GameObject.FindGameObjectsWithTag("ItemSpawn")[0];
        focusSphere.transform.position = new Vector3(focusSphere.transform.position.x, vrCamera.transform.position.y, focusSphere.transform.position.z);

    }



    public void StartGame()
    {
        if (isStudyRunning == true)
        {
            // We already have a study running, so we should end it before starting a new one
            if (finishedTrials == 0)
            {
                // This should never run
        
                OrientationTrials.TrialStart(this, 10, false, configOptions.orientationTimeToSpawnMin, configOptions.orientationTimeToSpawnMax);
            }
            if (finishedTrials == 1)
            {
                // Randomize the colors for orientation
                OrientationTrials.TrialStart(this, 10, true, configOptions.orientationTimeToSpawnMin, configOptions.orientationTimeToSpawnMax);
            }




            return;
        }

        stopwatch = new System.Diagnostics.Stopwatch();
        
        // Get UI text
        GameObject[] uiText = GameObject.FindGameObjectsWithTag("StartUI");
        
        //Hide UI
        foreach (GameObject text in uiText)
        {
            text.SetActive(false);
        }

        isStudyRunning = true;

        radius = configOptions.radiusOfObjectsMeters;
        distance = configOptions.distanceFromUserMeters;

        GenerateShapePoints();

        // Start the first trial
        OrientationTrials.TrialStart(this, 10, false, configOptions.orientationTimeToSpawnMin, configOptions.orientationTimeToSpawnMax);

        // Start the second trial
        //QuickColorMemory.TrialStart(this);

        // Start the second trial with the items changing
        //QuickColorMemory.TrialStart(this);
        
        // Wait until the project is done
    }


    public void GenerateShapePoints()
    {

        objects = new GameObject[configOptions.orientationOtherObjects.Length + configOptions.orientationDistractorObjects.Length + 1];

        objects[0] = configOptions.orientationTargetObject;
        configOptions.orientationDistractorObjects.CopyTo(objects, 1);
        configOptions.orientationOtherObjects.CopyTo(objects, configOptions.orientationDistractorObjects.Length + 1);

        numPoints = objects.Length;

        // Generating the points based on the polygon automatically, regardless of the number of points
        shapePositions = new Vector3[numPoints];

        // Generating the points based on the polygon automatically, regardless of the number of points
        for (int i = 0; i < numPoints; i++) 
        {
            if (configOptions.itemLocation == ItemLocation.OnTable)
            {
                shapePositions[i] = new Vector3((float)(radius * Math.Sin(2 * Math.PI * i / numPoints)), -0.5f, (float)(radius * Math.Cos(2 * Math.PI * i / numPoints)));
            }
            else if (configOptions.itemLocation == ItemLocation.InAir)
            {
                shapePositions[i] = new Vector3((float)(radius * Math.Sin(2 * Math.PI * i / numPoints)), (float)(radius * Math.Cos(2 * Math.PI * i / numPoints)),0);
            }   
        }


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
                OrientationTrials.PrimaryButtonPressed(this);
            }
        }
    }

    public void SecondaryButtonPressed()
    {
        if (isTrialRunning)
        {
            if (trial == Trial.Orientation)
            {
                OrientationTrials.SecondaryButtonPressed(this);
            }
        }
    }

    public void EndGame()
    {

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

    private void FixedUpdate() {
        // Get the time from the stopwatch
        if (isTrialRunning)
        {
            //long time_ms = stopwatch.ElapsedMilliseconds;
            // Check what is happening, see if there's any input.

            if (Input.GetKeyDown("space"))
            {
                // Log that in the file that we just started the trial
            }

            if (Input.GetKeyDown("up") || Input.GetKeyDown("down"))
            {
                // Log that in the file that we just pressed a button
            }
            
            
            
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
