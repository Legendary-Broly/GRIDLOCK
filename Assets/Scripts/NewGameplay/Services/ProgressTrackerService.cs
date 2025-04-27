using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using NewGameplay.Interfaces;
using NewGameplay.Services;

public class ProgressTrackerService : IProgressTrackerService
{
    public int CurrentScore { get; private set; }
    public int RoundTarget { get; private set; } = 20;  // Initial round target

    public int CurrentProgress => CurrentScore;
    public int CurrentThreshold => RoundTarget;

    public void ApplyScore(int score)
    {
        CurrentScore += score;
    }

    public bool HasMetGoal() => CurrentScore >= RoundTarget;

    public void ResetProgress()
    {
        CurrentScore = 0;
    }

    public void IncreaseThreshold()
    {
        Debug.Log("IncreaseThreshold called. Current RoundTarget: " + RoundTarget); // Debug log
        RoundTarget += 20;  // Increase threshold by 20 each round
        Debug.Log("Threshold increased. New RoundTarget: " + RoundTarget); // Debug log
    }
}

