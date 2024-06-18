using System;
using UnityEngine;

[Serializable]
public class OrientationBlockConfig
{
    public string configName;
    public GameObject targetObject;
    public GameObject[] otherObjects;
    public GameObject[] distractorObjects;
    public Color distracterItemColor;
    public Color regularItemColor;
    [Range(1, 6)]
    public float timeToSpawnMin = 2;
    [Range(1, 6)]
    public float timeToSpawnMax = 5;
    public Color orientationBackgroundColor; 
    public bool orientationRandomizeColors;
    public bool orientation2DShapes;
    public int orientationPercentageOfDistractors;
    public float radiusOfObjectsMeters;
    public float distanceFromUserMeters;
    public ItemLocation itemLocation;
    public float itemsScale;
    public bool changeColor;
    public bool changeItem;
    public int numberOfTrials;
}