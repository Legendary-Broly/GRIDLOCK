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
    void ApplyScore(int score);
    bool HasMetGoal();
    void ResetForNextRound();
}