using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
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


    // Start is called before the first frame update
    void Start()
    {

    }



    public void StartGame()
    {
        if (isStudyRunning == true)
        {
            // We already have a study running, so we should end it before starting a new one
            return;
        }

        isStudyRunning = true;

        radius = configOptions.radiusOfObjectsMeters;
        distance = configOptions.distanceFromUserMeters;

        GenerateShapePoints();

        // Start the first trial
        OrientationTrials.TrialStart(this);

        // Start the second trial
        //QuickColorMemory.TrialStart(this);

        // Start the second trial with the items changing
        //QuickColorMemory.TrialStart(this);
        
        // Wait until the project is done
    }


    public void GenerateShapePoints()
    {

        objects = new GameObject[configOptions.orientationOtherObjects.Length + configOptions.orientationDistractorObjects.Length + 1];

        objects[0] = configOptions.orientationMainObject;
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
                shapePositions[i] = new Vector3((float)(radius * Math.Cos(2 * Math.PI * i / numPoints)), -0.5f, (float)(radius * Math.Sin(2 * Math.PI * i / numPoints)));
            }
            else if (configOptions.itemLocation == ItemLocation.InAir)
            {
                shapePositions[i] = new Vector3((float)(radius * Math.Cos(2 * Math.PI * i / numPoints)), (float)(radius * Math.Sin(2 * Math.PI * i / numPoints)),0);
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
