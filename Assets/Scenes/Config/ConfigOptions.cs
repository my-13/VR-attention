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
    public GameObject mainObject;
    public GameObject[] otherObjects;
    public GameObject[] distractorObjects;
    public float radiusOfObjectsMeters;
    public float distanceFromUserMeters;
    public ItemLocation itemLocation;
    public float itemsScale = 1;
    public Color itemColor;
    public Color distracterItemColor;
    public Color regularItemColor;
}


