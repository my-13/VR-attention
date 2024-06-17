using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class OrientationTrialData
{
    public SaveData saveDataUsed;
    public int numberOfTrials = 0; // This should be equivalent to the number of trialTimes
    public List<float> trialTimesMiliseconds = new();
    public List<bool> wasCorrect = new();
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
    public static OrientationTrialData trials;
    private static bool isTrialRunning = false;
    [HideInInspector]
    public static GameObject linePrefab;
    [HideInInspector]
    public static GameObject trialObjectsParent;
    [HideInInspector]
    public static GameObject checkmark;
    [HideInInspector]
    public static ConfigOptions configOptions;
    [HideInInspector]
    public static Vector3[] shapePositions;
    [HideInInspector]
    public static Camera vrCamera;
    
    private static float distance;
    public static int numberOfTrials;
    public static bool randomizeColors;
    private static float radius;
    private static GameObject[] objects;


    public static void TrialStart(GameManager manager, int numOfTrials, bool randColors, float minSec = 2, float maxSec = 5)
    {
        stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        manager.trial = Trial.Orientation;
        trials = new OrientationTrialData();
        configOptions = manager.configOptions;
        vrCamera = manager.vrCamera;
        
        objects = manager.objects;
        shapePositions = manager.shapePositions;
        linePrefab = manager.linePrefab;
        trialObjectsParent = manager.trialObjectsParent;
        checkmark = manager.checkmark;
        
        numberOfTrials = numOfTrials;
        randomizeColors = randColors;

        radius = configOptions.radiusOfObjectsMeters;
        distance = configOptions.distanceFromUserMeters;

        manager.StartCoroutine( WaitForTrial(manager, true, randomizeColors, minSec, maxSec));
    }

    public static IEnumerator WaitForTrial(GameManager manager, bool wait, bool randomizeColors, float minSec = 2, float maxSec = 5)
    {   
        if (wait)
        {
            
            yield return new WaitForSeconds(UnityEngine.Random.Range(minSec,maxSec));
            RunOrientationTrial(manager, randomizeColors);
        }
        yield return new WaitForSeconds(0);
        RunOrientationTrial(manager, randomizeColors);
    }
    public static void RunOrientationTrial(GameManager manager, bool randomizeColors)
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
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("ItemSpawn");
        Vector3 spawnPos = spawnPoints[0].transform.position;
        
        // Calculate the vector from the camera to the center of the spawn point
        Vector3 cameraForward = (spawnPos - cameraPos).normalized * distance;
        Vector3 shapeCenter = cameraPos + cameraForward;

        // Shuffle the positions of the objects to randomize study
        var rng = new System.Random();
        GameManager.Shuffle(rng, shapePositions);

        // Get the starting time
        start_time_ms = stopwatch.ElapsedMilliseconds;
        

        Color normalColor = configOptions.orientationRegularItemColor;
        Color distractorColor = configOptions.orientationDistracterItemColor;


        if (randomizeColors)
        {
            Color[] colors = {normalColor, distractorColor};
            GameManager.Shuffle(rng, colors);
            normalColor = colors[0];
            distractorColor = colors[1];
        }

        // Summon The Objects
        for (int i = 0; i < shapePositions.Length; i++)
        {
            GameObject prefabObj = objects[i];
            GameObject obj = Instantiate(prefabObj, shapePositions[i] + shapeCenter, prefabObj.transform.rotation).gameObject;
            obj.transform.SetParent(trialObjectsParent.transform);
            obj.transform.localScale *= configOptions.itemsScale;


            if (i == 0)
            {
                itemOrientation = RandLineOrientation(obj);
                obj.GetComponent<Renderer>().material.color = normalColor;
            }
            else if (i < configOptions.orientationDistractorObjects.Length + 1)
            {
                RandLineOrientation(obj);
                obj.GetComponent<Renderer>().material.color = distractorColor;
            }
            else
            {
                RandLineOrientation(obj);
                obj.GetComponent<Renderer>().material.color = normalColor;
            }
        }
        
    }

    public static void StopOrientationTrial(GameManager manager, LineOrientation orientation)
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
        trials.wasCorrect.Add(wasCorrect);

        // Destroy all objects
        foreach (Transform transform in trialObjectsParent.transform)
        {
            Destroy(transform.gameObject);
        }

        if (trials.numberOfTrials >= configOptions.numberOfTrials)
        {
            TrialEnd(manager);
        }
        else
        {
            manager.StartCoroutine( WaitForTrial(manager, true, randomizeColors));
        }
    }

    public static LineOrientation RandLineOrientation(GameObject obj)
    {
        LineOrientation orientation = (LineOrientation) UnityEngine.Random.Range(0, 2);
        GameObject line = Instantiate(linePrefab.transform, obj.transform.position + (vrCamera.transform.forward * -0.065f * configOptions.itemsScale), linePrefab.transform.rotation).gameObject;
        line.transform.SetParent(trialObjectsParent.transform);
        if (orientation == LineOrientation.Horizontal)
        {
            line.transform.Rotate(0, 0, 90);
        }

        return orientation;
    }
    
    public static void PrimaryButtonPressed(GameManager manager)
    {
        if (isTrialRunning)
        {
            StopOrientationTrial(manager, LineOrientation.Vertical);
        }
    }

    public static void SecondaryButtonPressed(GameManager manager)
    {
        if (isTrialRunning)
        {
            StopOrientationTrial(manager,  LineOrientation.Horizontal);
        }
    }

    public static void TrialEnd(GameManager manager)
    {
        // Save the data to a file
        string json = JsonUtility.ToJson(OrientationTrials.trials);
        System.IO.File.WriteAllText("orientation_trials.json", json);

        Vector3 cameraPos = vrCamera.transform.position;
        Vector3 cameraForward = vrCamera.transform.forward * distance;
        Vector3 shapeCenter = cameraPos + cameraForward;

        Instantiate(checkmark, shapeCenter,Quaternion.identity).transform.SetParent(trialObjectsParent.transform);
        
        manager.StartCoroutine(manager.ClearTrialObjects());

        manager.EndGame();
        manager.trial = Trial.NoTrial;
        manager.isTrialRunning = false;
    }
}
