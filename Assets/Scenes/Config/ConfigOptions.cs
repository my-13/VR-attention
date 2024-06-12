using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemLocation
{
    OnTable,
    InAir
}

[Serializable]
[CreateAssetMenu(fileName = "ConfigOptions", menuName = "ScriptableObjects/ConfigOptions", order = 1)]
public class ConfigOptions : ScriptableObject
{
    public string configName;
    public GameObject orientationTargetObject;
    public GameObject[] orientationOtherObjects;
    public GameObject[] orientationDistractorObjects;
    public Color orientationDistracterItemColor;
    public Color orientationRegularItemColor;
    public Color orientationBackgroundColor; 
    public bool orientationRandomizeColors = false;
    public bool orientation2DShapes = false;
    
    public float radiusOfObjectsMeters;
    public float distanceFromUserMeters;
    public ItemLocation itemLocation;
    public float itemsScale = 1;


    public bool changeColor = true;
    public bool changeItem = false;

    public int numberOfTrials;

    public GameObject[] quickColorMemoryObjects;
}


