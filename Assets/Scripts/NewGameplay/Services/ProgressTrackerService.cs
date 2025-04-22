using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class ProgressTrackerService : IProgressTrackerService
{
    public int CurrentScore { get; private set; }
    public int RoundTarget { get; private set; } = 20;  // Initial round threshold

    public void ApplyScore(int score)
    {
        CurrentScore += score;
    }

    public bool HasMetGoal()
    {
        return CurrentScore >= RoundTarget;
    }

    public void IncreaseThreshold()
    {
        RoundTarget += 20;  // Increase threshold by 20 each round
    }
        public void ResetProgress()
    {
        CurrentScore = 0;
    }
}

