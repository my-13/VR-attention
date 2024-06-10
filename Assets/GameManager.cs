using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;



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


public class GameManager : MonoBehaviour
{
    // Time taken to complete the game
    private bool isTrialRunning = false;
    private bool isStudyRunning = false;
    public ConfigOptions configOptions;
    
    // Objects referenced to be passed to the trials scripts
    public GameObject linePrefab;
    public GameObject trialObjects;
    public GameObject checkmark;

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

    }

    IEnumerator WaitForTrial(bool wait, int minSec = 2, int maxSec = 5)
    {   
        if (wait)
        {
            
            yield return new WaitForSeconds(UnityEngine.Random.Range(minSec,maxSec));
            RunOrientationTrial();
        }
        yield return new WaitForSeconds(0);
        RunOrientationTrial();
    }

    public void RunOrientationTrial()
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
            GameObject obj = Instantiate(prefabObj, shapePositions[i] + shapeCenter, prefabObj.transform.rotation).gameObject;
            obj.transform.SetParent(trialObjects.transform);
            obj.transform.localScale *= configOptions.itemsScale;
            if (i == 0)
            {
                itemOrientation = RandLineOrientation(obj);
                obj.GetComponent<Renderer>().material.color = configOptions.itemColor;
            }
            else if (i < configOptions.distractorObjects.Length + 1)
            {
                RandLineOrientation(obj);
                obj.GetComponent<Renderer>().material.color = configOptions.distracterItemColor;
            }
            else
            {
                RandLineOrientation(obj);
                obj.GetComponent<Renderer>().material.color = configOptions.regularItemColor;
            }
        }
    }

    public void EndOrientationTrial(bool wasCorrect)
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
        trials.numberOfTrials++;
        trials.trialTimesMiliseconds.Add(time_ms);
        trials.wasCorrect.Add(wasCorrect);

        // Destroy all objects
        foreach (Transform transform in trialObjects.transform)
        {
            Destroy(transform.gameObject);
        }

        if (trials.numberOfTrials >= configOptions.numberOfTrials)
        {
            EndGame();
        }
        else
        {
            StartCoroutine(WaitForTrial(true));
        }
    }

    public void StartGame()
    {
        if (isStudyRunning == true)
        {
            // We already have a study running, so we should end it before starting a new one
            RunOrientationTrial();
            return;
        }
        isStudyRunning = true;
        
        trials = new Trials();

        // Collecting data from UI Options
        radius = configOptions.radiusOfObjectsMeters;
        distance = configOptions.distanceFromUserMeters;
        objects = new GameObject[configOptions.otherObjects.Length + configOptions.distractorObjects.Length + 1];
        
        objects[0] = configOptions.mainObject;
        configOptions.distractorObjects.CopyTo(objects, 1);
        configOptions.otherObjects.CopyTo(objects, configOptions.distractorObjects.Length + 1);

        //objects = configOptions.otherObjects;
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
        
        StartCoroutine(WaitForTrial(false));

    }

    public LineOrientation RandLineOrientation(GameObject obj)
    {
        LineOrientation orientation = (LineOrientation) UnityEngine.Random.Range(0, 2);
        GameObject line = Instantiate(linePrefab.transform, obj.transform.position + (vrCamera.transform.forward * -0.05f * configOptions.itemsScale), linePrefab.transform.rotation).gameObject;
        line.transform.SetParent(trialObjects.transform);
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
            if (itemOrientation == LineOrientation.Vertical)
            {
                EndOrientationTrial(true);
            }
            else
            {
                EndOrientationTrial(false);
            }
        }
    }

    public void SecondaryButtonPressed()
    {
        if (isTrialRunning)
        {
            // Collect data here
            if (itemOrientation == LineOrientation.Horizontal)
            {
                EndOrientationTrial(true);
            }
            else
            {
                EndOrientationTrial(false);
            }
        }
    }

    void EndGame()
    {
        // Save the data to a file
        string json = JsonUtility.ToJson(trials);
        System.IO.File.WriteAllText("trials.json", json);

        Vector3 cameraPos = vrCamera.transform.position;
        Vector3 cameraForward = vrCamera.transform.forward * distance;
        Vector3 shapeCenter = cameraPos + cameraForward;

        Instantiate(checkmark, shapeCenter,Quaternion.identity).transform.SetParent(trialObjects.transform);
        
        StartCoroutine(ClearTrialObjects());

        isStudyRunning = false;
    }

    public IEnumerator ClearTrialObjects()
    {
        
        yield return new WaitForSeconds(2);
        foreach (Transform transform in trialObjects.transform)
        {
            Destroy(transform.gameObject);
        }
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
