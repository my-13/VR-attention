using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Time taken to complete the game
    private int time_ms = 0;
    private int start_time_ms = 0;

    // Polygon parameters
    public int numPoints = 6;
    public float radius = 1;


    // List of objects to be spawned
    public GameObject[] objects;

    // List of objects that are included in that list determined from the options
    private GameObject[] included_objects;

    private Vector3[] positions;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Hello World!");
        included_objects = objects;
        StartGame();
    }

    void StartGame()
    {
        // Get the starting time
        start_time_ms = System.DateTime.Now.Millisecond;


        // Generating the points based on the polygon automatically, regardless of the number of points
        double angle = 2 * Math.PI / numPoints;
        positions = new Vector3[numPoints];
        for (int i = 0; i < numPoints; i++)
        {

            positions[i] = new Vector3((float)(radius * Math.Asin(i * angle)), (float)(radius * Math.Acos(i * angle)), 0);
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
        int end_time_ms = System.DateTime.Now.Millisecond;

        // Calculate the time taken
        time_ms = end_time_ms - start_time_ms;

        // Print the time taken
        Debug.Log("Time taken: " + time_ms + "ms");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
