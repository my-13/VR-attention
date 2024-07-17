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

    private void Start() {
        foreach (var block in orientationBlocks)
        {
            block.configOptions = this;
        }    
    }

    public bool IsLastBlock(){
        return currentBlock == procedureConfig.procedureBlocks.Length - 1;
    }

    public void SetButtonBlockConfigData(ProcedureConfig procedureConfig){
        GetButtonPressBlockConfig().numberOfTrials = procedureConfig.procedureBlocks[procedureConfig.currentBlock].Length;
    }

    public void SetReachingBlockConfigData(ProcedureConfig procedureConfig){
        GetReachingBlockConfig().numberOfTrials = procedureConfig.procedureBlocks[procedureConfig.currentBlock].Length;
    }

    public bool IsBlockAvailable(){
        return currentBlock < procedureConfig.procedureBlocks.Length ;
    }

    public OrientationBlockConfig GetButtonPressBlockConfig(){
        return orientationBlocks[0];
    }

    public OrientationBlockConfig GetReachingBlockConfig(){
        return orientationBlocks[1];
    }

    public OrientationBlockConfig GetNextBlockConfig(){
        currentBlock++;

        return GetCurrentBlockConfig();
    }

    public OrientationBlockConfig GetCurrentBlockConfig(){
        OrientationBlockConfig currentBlockConfig = orientationBlocks[currentBlock];
        currentBlockConfig.numberOfTrials = procedureConfig.procedureBlocks[currentBlock].Length;
        currentBlockConfig.feedbackType = procedureConfig.procedureBlocks[currentBlock][0][0] == '0' ? FeedbackType.ButtonInput : FeedbackType.Reaching;
        currentBlockConfig.isItemsRealistic = procedureConfig.procedureBlocks[currentBlock][0][1] == '0' ? false : true;
        
        return currentBlockConfig;
    }
}


