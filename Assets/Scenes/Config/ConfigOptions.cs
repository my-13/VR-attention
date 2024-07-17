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
    public ProcedureConfig procedureConfig;

    public bool IsLastBlock(){
        return currentBlock == orientationBlocks.Length - 1;
    }

    public void SetButtonBlockConfigData(ProcedureConfig procedureConfig){
        GetButtonPressBlockConfig().numberOfTrials = procedureConfig.procedureBlocks[procedureConfig.currentBlock].Length;
    }

    public void SetReachingBlockConfigData(ProcedureConfig procedureConfig){
        GetReachingBlockConfig().numberOfTrials = procedureConfig.procedureBlocks[procedureConfig.currentBlock].Length;
    }

    public bool IsBlockAvailable(){
        return currentBlock < orientationBlocks.Length;
    }

    public OrientationBlockConfig GetButtonPressBlockConfig(){
        return orientationBlocks[0];
    }

    public OrientationBlockConfig GetReachingBlockConfig(){
        return orientationBlocks[1];
    }

    public OrientationBlockConfig GetNextBlockConfig(){
        currentBlock++;

        return orientationBlocks[currentBlock];
    }

    public OrientationBlockConfig GetCurrentBlockConfig(){
        return orientationBlocks[currentBlock];
    }
}


