using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[Serializable]
[CreateAssetMenu(fileName = "ConfigOptions", menuName = "ScriptableObjects/ConfigOptions", order = 1)]
public class ConfigOptions : ScriptableObject
{
    public string configName;

    public OrientationBlockConfig[] orientationBlocks;
    public ProcedureConfig procedureConfig;

    private void Start() {
        foreach (var block in orientationBlocks)
        {
            block.configOptions = this;
        }    
    }

    public bool IsLastBlock(){
        return procedureConfig.currentTrial == procedureConfig.procedureBlocks.Length - 1;
    }

    public void SetButtonBlockConfigData(ProcedureConfig procedureConfig){
        GetButtonPressBlockConfig().numberOfTrials = procedureConfig.procedureBlocks[procedureConfig.currentBlock].Length;
    }

    public void SetReachingBlockConfigData(ProcedureConfig procedureConfig){
        GetReachingBlockConfig().numberOfTrials = procedureConfig.procedureBlocks[procedureConfig.currentBlock].Length;
    }

    public bool IsBlockAvailable(){
        return procedureConfig.currentBlock < procedureConfig.procedureBlocks.Length ;
    }

    public OrientationBlockConfig GetButtonPressBlockConfig(){
        return orientationBlocks[0];
    }

    public OrientationBlockConfig GetReachingBlockConfig(){
        return orientationBlocks[1];
    }

    public OrientationBlockConfig GetNextBlockConfig(){
        procedureConfig.currentBlock++;

        return GetCurrentBlockConfig();
    }

    public OrientationBlockConfig GetCurrentBlockConfig(){
        // Never ever let me cook again
        OrientationBlockConfig currentBlockConfig = GetButtonPressBlockConfig();

        if (procedureConfig.GetCurrentFeedbackType() == FeedbackType.ButtonInput) {
            currentBlockConfig =  GetButtonPressBlockConfig();
        } else {
            currentBlockConfig =  GetReachingBlockConfig();
        }
        currentBlockConfig.numberOfTrials = procedureConfig.procedureBlocks[procedureConfig.currentBlock].Length;
        currentBlockConfig.isMainColorSwapped = procedureConfig.procedureBlocks[procedureConfig.currentBlock][0][2] == '0' ? false : true;
        currentBlockConfig.feedbackType = procedureConfig.procedureBlocks[procedureConfig.currentBlock][0][0] == '0' ? FeedbackType.ButtonInput : FeedbackType.Reaching;
        currentBlockConfig.isItemsRealistic = procedureConfig.procedureBlocks[procedureConfig.currentBlock][0][1] == '0' ? false : true;
        
        return currentBlockConfig;
    }
}


