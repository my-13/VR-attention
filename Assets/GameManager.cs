using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LineOrientation
{
    Horizontal,
    Vertical
}

[Serializable]
public class SaveData
{
    public int numberOfPoints;
    public float polygonRadius;
    public float distanceToPolygon;
    public string objectTypes;
    public int numberOfDistractions;
}

[Serializable]
public class TimeTrials
{
    public SaveData saveDataUsed;
    public float timeTakenMS;
    public Vector3[] leftHandPosition;
    public Vector3[] rightHandPosition;
    public Vector3[] leftHandVelocity;
    public Vector3[] rightHandVelocity;
}

public class GameManager : MonoBehaviour
{
    // Time taken to complete the game
    private System.Diagnostics.Stopwatch stopwatch;
    private long time_ms = 0;
    private long start_time_ms = 0;
    public bool isTrialRunning = false;
    public bool isStudyRunning = false;
    public LineOrientation itemOrientation;
    
    public ConfigOptions configOptions;
    public GameObject linePrefab;

    // Polygon parameters
    private int numPoints;
    private float radius;
    private float distance;

    // List of objects to be spawned
    public GameObject[] objects;
    private Vector3[] shapePositions;

    public Camera vrCamera;

    // Start is called before the first frame update
    void Start()
    {
        // UI Objects
        stopwatch = new System.Diagnostics.Stopwatch();
    }

    public void RunTrial()
    {
        if (isTrialRunning == true)
        {
            // Wait until the trial is over before starting a new one
            return;
        }
        isTrialRunning = true;

        // Get the camera position and forward vector
        Vector3 cameraPos = vrCamera.transform.position;
        Vector3 cameraForward = vrCamera.transform.forward * distance;
        Vector3 shapeCenter = cameraPos + cameraForward;
        
        // Shuffle the positions of the objects to randomize study
        var rng = new System.Random();
        Shuffle(rng, shapePositions);

        // Get the starting time
        stopwatch.Start();
        start_time_ms = stopwatch.ElapsedMilliseconds;

        // Summon The Objects
        for (int i = 0; i < shapePositions.Length; i++)
        {
            GameObject prefabObj = objects[i];
            GameObject obj = Instantiate(prefabObj.transform, shapePositions[i] + shapeCenter, prefabObj.transform.rotation).gameObject;
            obj.transform.localScale *= configOptions.itemsScale;
            if (i == 0)
            {
                itemOrientation = RandLineOrientation(obj);
                obj.GetComponent<Renderer>().material.color = configOptions.itemColor;
            }
            else
            {
                
                RandLineOrientation(obj);
                obj.GetComponent<Renderer>().material.color = configOptions.regularItemColor;
            }
        }
    }

    public void EndTrial()
    {
        // Collect data here
        // Get the ending time
        stopwatch.Stop();
        long end_time_ms = stopwatch.ElapsedMilliseconds;

        // Calculate the time taken
        time_ms = end_time_ms - start_time_ms;

        // Print the time taken
        Debug.Log("Time taken: " + time_ms + "ms");
        isTrialRunning = false;
    }

    public void StartGame()
    {
        if (isStudyRunning == true)
        {
            // We already have a study running, so we should end it before starting a new one
            RunTrial();
            return;
        }
        isStudyRunning = true;

        // Collecting data from UI Options
        radius = configOptions.radiusOfObjectsMeters;
        distance = configOptions.distanceFromUserMeters;
        objects = new GameObject[configOptions.otherObjects.Length + configOptions.distractorObjects.Length + 1];
        objects = configOptions.otherObjects;
        numPoints = objects.Length;

        // Generating the points based on the polygon automatically, regardless of the number of points
        shapePositions = new Vector3[numPoints];

        // Generating the points based on the polygon automatically, regardless of the number of points
        for (int i = 0; i < numPoints; i++) 
        {
            if (configOptions.itemLocation == ItemLocation.OnTable)
            {
                shapePositions[i] = new Vector3((float)(radius * Math.Cos(2 * Math.PI * i / numPoints)), -0.5f, (float)(radius * Math.Sin(2 * Math.PI * i / numPoints)));
            }
            else if (configOptions.itemLocation == ItemLocation.InAir)
            {
                shapePositions[i] = new Vector3((float)(radius * Math.Cos(2 * Math.PI * i / numPoints)), (float)(radius * Math.Sin(2 * Math.PI * i / numPoints)),0);
            }
            
        }
        
        RunTrial();

    }

    public LineOrientation RandLineOrientation(GameObject obj)
    {
        LineOrientation orientation = (LineOrientation) UnityEngine.Random.Range(0, 2);
        GameObject line = Instantiate(linePrefab.transform, obj.transform.position + (vrCamera.transform.forward * -0.05f * configOptions.itemsScale), linePrefab.transform.rotation).gameObject;
        if (orientation == LineOrientation.Horizontal)
        {
            line.transform.Rotate(0, 0, 90);
        }

        return orientation;
    }
    
    public void StartButtonPressed()
    {
        StartGame();
    }
    
    public void PrimaryButtonPressed()
    {
        if (isTrialRunning)
        {
            // Collect data here

        }
    }

    public void SecondaryButtonPressed()
    {
        if (isTrialRunning)
        {
            // Collect data here

        }
    }

    void EndGame()
    {
        
        isStudyRunning = false;
    }

    // Helper functions
    
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
