using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
public interface IProgressTrackerService
{
    int CurrentScore { get; }
    int RoundTarget { get; }
    int CurrentProgress { get; }
    int CurrentThreshold { get; }

    void ApplyScore(int score);
    bool HasMetGoal();
    void ResetProgress();
    void IncreaseThreshold();
}