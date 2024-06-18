using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Search;
using Unity.VisualScripting;

[Serializable]
public class ColorMemoryTrialData
{
    public SaveData saveDataUsed;
    public int numberOfTrials = 0; // This should be equivalent to the number of trialTimes
    public List<float> trialTimesMiliseconds = new();
    public List<Color> colorChanged = new();
    public List<bool> wasCorrect = new();
}


public class QuickColorMemory : MonoBehaviour
{
    
    public static GameObject[] objects;
    public static ConfigOptions configOptions;
    public static Camera vrCamera;
    public static ColorMemoryTrialData trials;
    public static GameObject checkmark;

    public static GameObject changedObject;

    public static bool changeItem;
    public static bool changeColor;
    public static GameObject trialObjectsParent;
    
    public static void TrialStart(GameManager manager, bool changeItem = false, bool changeColor = false)
    {
        // This will use exactly 6 objects, usually with random

        //objects = manager.configOptions.quickColorMemoryObjects;
        manager.trial = Trial.ColorMemory;
        configOptions = manager.configOptions;
        vrCamera = manager.vrCamera;
        trials = new ColorMemoryTrialData();
        QuickColorMemory.changeColor = manager.configOptions.changeColor || changeColor;
        QuickColorMemory.changeItem = manager.configOptions.changeItem || changeItem;
        checkmark = manager.checkmark;
        trialObjectsParent = manager.trialObjectsParent;
        // Start the trial


    }

    public static void RunColorMemoryTrial(GameManager manager)
    {
        // Show the objects for 0.25 seconds, then hide them
        
        // Select one object that will change color, and save

        // Wait for the user to select the object by reaching out and grabbing it


    }

    public static IEnumerator ShowObjects(GameManager manager)
    {
        // Show the objects for 0.25 seconds
        foreach (var obj in objects)
        {
            obj.SetActive(true);
        }
        yield return new WaitForSeconds(0.25f);
        foreach (var obj in objects)
        {
            obj.SetActive(false);
        }
    }

    public static void StopColorMemoryTrial(GameManager manager, GameObject grabbedObject)
    
    {
        // On Grab, check if the object is the correct one
        if (changedObject == grabbedObject)
        {
            trials.wasCorrect.Add(true);
            trials.colorChanged.Add(changedObject.GetComponent<Renderer>().material.color);

        }
        else
        {
            trials.wasCorrect.Add(false);
            trials.colorChanged.Add(changedObject.GetComponent<Renderer>().material.color);
        }

        // See if there is more trials to run
        if (trials.numberOfTrials < configOptions.numberOfTrials)
        {
            // Run the next trial, after they press any button on
            
            RunColorMemoryTrial(manager);
        }
        else
        {
            // End the trial
            TrialEnd();
        }

        // Stop the trial
    }

    public static void TrialEnd()
    {

    }

    
}
