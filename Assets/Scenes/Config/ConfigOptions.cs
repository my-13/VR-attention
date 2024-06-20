using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;



[Serializable]
[CreateAssetMenu(fileName = "ConfigOptions", menuName = "ScriptableObjects/ConfigOptions", order = 1)]
public class ConfigOptions : ScriptableObject
{
    public string configName;

    public OrientationBlockConfig[] orientationBlocks;
    public int currentBlock = 0;

    public OrientationBlockConfig getNextBlockConfig(){
        currentBlock++;

        return orientationBlocks[currentBlock];
    }

    public OrientationBlockConfig getCurrentBlockConfig(){
        return orientationBlocks[currentBlock];
    }
}


