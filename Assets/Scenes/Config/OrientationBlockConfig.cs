using System;
using UnityEngine;

public enum ItemLocation
{
    OnTable,
    InAir
}

[Serializable]
[CreateAssetMenu(fileName = "OrientationBlockConfig", menuName = "ScriptableObjects/OrientationBlockConfig", order = 2)]
public class OrientationBlockConfig: ScriptableObject
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
    public Color backgroundColor = Color.black; 
    public bool randomizeColors = false;
    public bool has2DShapes = false;
    [Range(0, 100)]
    public int randomPercentageOfDistractor = 50;
    public float radiusOfObjectsMeters = 0.25f;
    public float distanceFromUserMeters = 0.6f;
    public ItemLocation itemLocation;
    [Range(0.01f, 10)]
    public float itemsScale = 1;
    public bool changeItemPosition = false;
    public int numberOfTrials = 10;
    [Range(0, 180)]
    public int rotationDegrees = 0;
    public bool randomrotation = false;
}
