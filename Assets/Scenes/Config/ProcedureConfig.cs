using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;


[Serializable]
public class ProcedureConfig
{
    public string configName;
    private int currentBlock = 0;
    private int currentTrial = -1;
    private string[][] procedureBlocks;

    // Construct ProcedureConfig object
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
    
    public bool IsLastBlock() { return currentBlock >= procedureBlocks.Length - 1; }
    public bool IsLastTrial() { return currentTrial >= procedureBlocks[currentBlock].Length - 1; }
    public bool IsBlockAvailable() { return currentBlock < procedureBlocks.Length; }
    public bool IsTrialAvailable() { return currentTrial < procedureBlocks[currentBlock].Length; }
    
    public static void Shuffle<T> (System.Random rng, T[] array)
    {
        int n = array.Length;
        while (n > 1) 
        {
            int k = rng.Next(n--);
            (array[k], array[n]) = (array[n], array[k]);
        }
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


    public string GetCurrentTrialString()
    {
        return new string(procedureBlocks[currentBlock][currentTrial].Where(c => !Char.IsWhiteSpace(c)).ToArray());
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
}


