using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;


[Serializable]
public class ProcedureConfig
{
    // Name of Configuration
    public string configName;
    // Number of Current Block
    private int currentBlock = 0;
    // Number of Current Trial
    private int currentTrial = -1;
    // List of strings holding the procedures as strings
    private string[][] procedureBlocks;
    // Orientation Block Config
    private OrientationBlockConfig orientationBlockConfig;

    // Constructor
    public ProcedureConfig(string configName, string path)
    {
        this.configName = configName;
        ReadProcedureFile(path);

        System.Random rng = new();
        for (int i = 0; i < procedureBlocks.Length; i++)
        {
            ProcedureConfig.Shuffle(rng, procedureBlocks[i]);
        }

        ProcedureConfig.Shuffle(rng, procedureBlocks);
        
        this.currentBlock = 0;
        this.currentTrial = 0;
    }

    public string GetNextTrialString()
    {
        currentTrial++;
        if (currentTrial >= procedureBlocks[currentBlock].Length)
        {
            currentTrial = 1;
            currentBlock++;
        }
        return procedureBlocks[currentBlock][currentTrial];
    }

    public int GetCurrentTrialNumber() { return this.currentTrial; }
    public int GetCurrentBlockNumber() { return this.currentBlock; }
    public bool IsLastBlock() { return this.currentBlock >= this.procedureBlocks.Length - 1; }
    public bool IsLastTrial() { return this.currentTrial >= this.procedureBlocks[currentBlock].Length - 1; }
    public bool IsBlockAvailable() { return this.currentBlock < this.procedureBlocks.Length; }
    public bool IsTrialAvailable() { return this.currentTrial < this.procedureBlocks[currentBlock].Length; }

    public OrientationBlockConfig GetOrientationBlockConfig(){ return this.orientationBlockConfig; }
    public string GetTrialString(int block, int trial){ return this.procedureBlocks[block][trial]; }
    public string[] GetTrialArray(int block, int trial) { return this.procedureBlocks[block][trial].Split(","); }
    public FeedbackType GetTrialFeedbackType(int block, int trial) { return GetTrialArray(block, trial)[0] == "0" ? FeedbackType.ButtonInput : FeedbackType.Reaching; }
    public LineOrientation GetTrialOrientation(int block, int trial) { return GetTrialArray(block, trial)[1] == "0" ? LineOrientation.Horizontal : LineOrientation.Vertical; }
    public bool GetTrialMainColor(int block, int trial) { return GetTrialArray(block, trial)[2] == "0"; }
    public bool GetTrialDistractor(int block, int trial) { return GetTrialArray(block, trial)[3] == "1"; }

    private void ReadProcedureFile(string path){
        StreamReader reader = new StreamReader(path);
        string file = reader.ReadToEnd();
        string[] lines = file.Split(new char[] {'\n'});  
        int count = lines.Length;
        string[] blocks = file.Split(new char[] {'#'});
        int numberOfBlocks = blocks.Length - 1;

        List<string>[] tempProceduresBlocks = new List<string>[numberOfBlocks];

        for (int i = 0; i < numberOfBlocks; i++)
        {
            tempProceduresBlocks[i] = new List<string>();
        }

        int tempCurrent = -1;
        for (int i = 0; i < count; i++)
        {
            var line = lines[i];
            if (i == 0){
                continue;
            }
            if (line[0] == '#')
            {
                tempCurrent++;
                continue;
            }
            tempProceduresBlocks[tempCurrent].Add(line.Trim());
        }
        
        procedureBlocks = new string[numberOfBlocks][];
        for (int i = 0; i < numberOfBlocks; i++)
        {
            procedureBlocks[i] = tempProceduresBlocks[i].ToArray();
        }

        reader.Close();
    }

    private void Shuffle<T> (System.Random rng, T[] array)
    {
        int n = array.Length;
        while (n > 1) 
        {
            int k = rng.Next(n--);
            (array[k], array[n]) = (array[n], array[k]);
        }
    }
}


