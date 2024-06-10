using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TrialsData
{
    public SaveData saveDataUsed;
    public int numberOfTrials = 0; // This should be equivalent to the number of trialTimes
    public List<float> trialTimesMiliseconds = new();
    public List<bool> wasCorrect = new();
}

public enum LineOrientation
{
    Horizontal,
    Vertical
}

public class OrientationTrials : MonoBehaviour
{

    private System.Diagnostics.Stopwatch stopwatch;
    private long time_ms = 0;
    private long start_time_ms = 0;
    private LineOrientation itemOrientation;
    private TrialsData trials;


    public void TrialStart()
    {
        stopwatch = new System.Diagnostics.Stopwatch();
        
    }

    public void RunOrientationTrial()
    {
        stopwatch.Start();
        start_time_ms = stopwatch.ElapsedMilliseconds;
    }

    public void StopOrientationTrial()
    {
        stopwatch.Stop();
        time_ms = stopwatch.ElapsedMilliseconds;
        stopwatch.Reset();

        // Save the time taken to complete the trial
        trials.trialTimesMiliseconds.Add(time_ms - start_time_ms);

        // Save the orientation of the item
        
        // Save the correctness of the answer

        // Check if the trial is the last one
    }

    public void TrialEnd()
    {

    }
}
