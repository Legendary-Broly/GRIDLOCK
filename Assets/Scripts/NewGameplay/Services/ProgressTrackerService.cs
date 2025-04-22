using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class ProgressTrackerService : IProgressTrackerService
{
    public int CurrentScore { get; private set; }
    public int RoundTarget { get; private set; } = 20;

    public void ApplyScore(int score)
    {
        CurrentScore += score;
    }

    public bool HasMetGoal() => CurrentScore >= RoundTarget;

    public void ResetForNextRound()
    {
        CurrentScore = 0;
        RoundTarget += 10; // progressively harder
    }
}