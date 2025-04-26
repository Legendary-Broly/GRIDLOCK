using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace NewGameplay.Interfaces
{
    public interface IEntropyService
    {
        event Action<float, bool> OnEntropyChanged;  // float is entropy value, bool indicates if this was a reset from 100%
        int EntropyPercent { get; }
        void Increase(int amount);
        void Decrease(int amount);
        float CurrentEntropy { get; }
        void ModifyEntropy(float amount);
        void DoubleVirusGrowthRate();
        void ResetVirusGrowthRate();
        int GetVirusGrowthRate();
    }
}