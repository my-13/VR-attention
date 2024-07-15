using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;



[Serializable]
[CreateAssetMenu(fileName = "ProcedureConfig", menuName = "ScriptableObjects/ProcedureConfig", order = 2)]
public class ProcedureConfig : ScriptableObject
{
    public string configName;
    public int currentBlock;
    public int currentTrial;
    public string[][] procedureBlocks;

    public bool IsLastBlock() { return currentBlock == procedureBlocks.Length - 1; }
    public bool IsLastTrial() { return currentTrial == procedureBlocks[currentBlock].Length - 1; }
    public bool IsBlockAvailable() { return currentBlock < procedureBlocks.Length; }
    public bool IsTrialAvailable() { return currentTrial < procedureBlocks[currentBlock].Length; }

    public string GetNextTrialString()
    {
        currentBlock++;
        currentTrial = 0;
        return procedureBlocks[currentBlock][currentTrial];
    }

    public string GetCurrentTrialString()
    {
        return procedureBlocks[currentBlock][currentTrial];
    }

    public string[] GetCurrentTrialArray()
    {
        return GetCurrentTrialString().Split(",");
    }

    public FeedbackType GetCurrentFeedbackType()
    {
        return GetCurrentTrialArray()[0] == "0" ? FeedbackType.ButtonInput : FeedbackType.Reaching;
    }

    public LineOrientation GetCurrentOrientation(){
        return GetCurrentTrialArray()[1] == "0" ? LineOrientation.Horizontal : LineOrientation.Vertical;
    }

    public bool GetCurrentMainColor(){
        return GetCurrentTrialArray()[2] == "0";
    }

    public bool GetCurrentDistractor(){
        return GetCurrentTrialArray()[3] == "1";
    }




}


