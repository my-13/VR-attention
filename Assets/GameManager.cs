using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}

public class GameManager : MonoBehaviour
{
    // Time taken to complete the game
    private System.Diagnostics.Stopwatch stopwatch;
    private long time_ms = 0;
    private long start_time_ms = 0;

    // Options canvas
    public GameObject options;

    // Polygon parameters
    private GameObject pointsSlider;
    private int numPoints;
    private GameObject radiusSlider;
    private float radius;

    private GameObject distanceSlider;
    private float distance;

    // List of objects to be spawned
    public GameObject[] objects;

    // List of objects that are included in that list determined from the options
    private GameObject[] included_objects;

    private Vector3[] shapePositions;

    // Start is called before the first frame update
    void Start()
    {
        // UI Objects
        pointsSlider = options.transform.Find("PointsSlider").gameObject;
        radiusSlider = options.transform.Find("RadiusSlider").gameObject;
        distanceSlider = options.transform.Find("DistanceSlider").gameObject;
        stopwatch = new System.Diagnostics.Stopwatch();

    

        Debug.Log("Hello World!");
        included_objects = objects;
        StartGame();
    }

    public void StartGame()
    {

        // Collecting data from UI Options
        numPoints = (int) pointsSlider.GetComponent<SliderTextUpdate>().value;
        radius = radiusSlider.GetComponent<SliderTextUpdate>().value;
        distance = distanceSlider.GetComponent<SliderTextUpdate>().value;

        // Get the starting time
        stopwatch.Start();
        start_time_ms = stopwatch.ElapsedMilliseconds;


        // Generating the points based on the polygon automatically, regardless of the number of points
        
        shapePositions = new Vector3[numPoints];


        // Generating the points based on the polygon automatically, regardless of the number of points
        for (int i = 0; i < numPoints; i++) 
        {
            shapePositions[i] = new Vector3((float)(radius * Math.Cos(2 * Math.PI * i / numPoints)), 0, (float)(radius * Math.Sin(2 * Math.PI * i / numPoints)));
        }
        
        
        // Summon
        /*for (int i = 0; i < included_objects.Length; i++)
        {
            GameObject prefabObj = included_objects[i];
            GameObject obj = Instantiate(prefabObj, positions[i] + new Vector3(0,2,0), Quaternion.identity);

        }*/
    }

    void EndGame()
    {
        // Get the ending time
        stopwatch.Stop();
        long end_time_ms = stopwatch.ElapsedMilliseconds;

        // Calculate the time taken
        time_ms = end_time_ms - start_time_ms;

        // Print the time taken
        Debug.Log("Time taken: " + time_ms + "ms");
    }

}
