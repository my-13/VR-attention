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


    public bool IsLastBlock(){
        return currentBlock == orientationBlocks.Length - 1;
    }
    public OrientationBlockConfig GetNextBlockConfig(){
        currentBlock++;

        return orientationBlocks[currentBlock];
    }

    public OrientationBlockConfig GetCurrentBlockConfig(){
        return orientationBlocks[currentBlock];
    }
}


