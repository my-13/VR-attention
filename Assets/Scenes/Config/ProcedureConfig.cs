using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;


[Serializable]
[CreateAssetMenu(fileName = "ProcedureConfig", menuName = "ScriptableObjects/ProcedureConfig", order = 2)]
public class ProcedureConfig
{
    public string configName;
    public int currentBlock = 0;
    public int currentTrial = 0;
    public string[][] procedureBlocks;

    public bool IsLastBlock() { return currentBlock == procedureBlocks.Length - 1; }
    public bool IsLastTrial() { return currentTrial == procedureBlocks[currentBlock].Length - 1; }
    public bool IsBlockAvailable() { return currentBlock < procedureBlocks.Length; }
    public bool IsTrialAvailable() { return currentTrial < procedureBlocks[currentBlock].Length; }

    // Construct ProcedureConfig object
    public ProcedureConfig(string configName, string path)
    {
        this.configName = configName;
        ReadProcedureFile(path);

        System.Random rng = new();
        for (int i = 0; i < procedureBlocks.Length; i++)
        {
            ProcedureConfig.Shuffle(rng, procedureBlocks[i]);
            //procedureBlocks[i] = procedureBlocks[i].OrderBy(a => rng.Next()).ToArray();
        }

        //procedureBlocks = procedureBlocks.OrderBy(a => rng.Next()).ToArray();
        ProcedureConfig.Shuffle(rng, procedureBlocks);
        this.currentBlock = 0;
        this.currentTrial = 0;
    }
    
    public static void Shuffle<T> (System.Random rng, T[] array)
    {
        int n = array.Length;
        while (n > 1) 
        {
            int k = rng.Next(n--);
            (array[k], array[n]) = (array[n], array[k]);
        }
    }


    void ReadProcedureFile(string path){
        StreamReader reader
            = new StreamReader(path);
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

        int currentBlock = -1;
        for (int i = 0; i < count; i++)
        {
            var line = lines[i];
            if (i == 0){
                continue;
            }
            if (line[0] == '#')
            {
                currentBlock++;
                continue;
            }
            tempProceduresBlocks[currentBlock].Add(line);
        }
        
        procedureBlocks = new string[numberOfBlocks][];
        for (int i = 0; i < numberOfBlocks; i++)
        {
            procedureBlocks[i] = tempProceduresBlocks[i].ToArray();
        }

        reader.Close();
    }

    public string GetNextTrialString()
    {
        currentTrial++;
        if (currentTrial >= procedureBlocks[currentBlock].Length)
        {
            currentTrial = 0;
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




}


