using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using NewGameplay.Interfaces;

namespace NewGameplay.Services
{
    public class EntropyService : IEntropyService
    {
        public event Action<float, bool> OnEntropyChanged; // bool indicates if this was a reset from 100%
        public int EntropyPercent { get; private set; } = 0;
        public float CurrentEntropy => EntropyPercent / 100f;
        private bool shouldResetOnNextChange = false;
        private int ignoreNextChanges = 0; // Number of changes to ignore after reset
        private const int CHANGES_TO_IGNORE = 3; // How many changes to ignore after reset

        public void Increase(int amount)
        {
            Debug.Log($"[EntropyService] Increase called with amount: {amount}. Current: {EntropyPercent}%, ShouldReset: {shouldResetOnNextChange}, IgnoreNext: {ignoreNextChanges}");
            int oldValue = EntropyPercent;
            bool wasReset = false;
            
            if (shouldResetOnNextChange)
            {
                Debug.Log("[EntropyService] Resetting entropy to 0% due to shouldResetOnNextChange flag");
                EntropyPercent = 0;
                shouldResetOnNextChange = false;
                wasReset = true;
                ignoreNextChanges = CHANGES_TO_IGNORE;
                Debug.Log($"[EntropyService] Will ignore next {CHANGES_TO_IGNORE} entropy changes");
            }
            else if (ignoreNextChanges > 0)
            {
                Debug.Log($"[EntropyService] Ignoring entropy change ({ignoreNextChanges} more to ignore)");
                ignoreNextChanges--;
                return;
            }
            else
            {
                int newValue = EntropyPercent + amount;
                if (newValue >= 100)
                {
                    Debug.Log("[EntropyService] Entropy would exceed 100%, setting to 100% and enabling reset flag");
                    EntropyPercent = 100;
                    shouldResetOnNextChange = true;
                }
                else
                {
                    EntropyPercent = newValue;
                }
            }
            
            if (oldValue != EntropyPercent)
            {
                Debug.Log($"[EntropyService] Entropy changed from {oldValue}% to {EntropyPercent}% (WasReset: {wasReset})");
                OnEntropyChanged?.Invoke(EntropyPercent, wasReset);
            }
        }

        public void Decrease(int amount)
        {
            Debug.Log($"[EntropyService] Decrease called with amount: {amount}. Current: {EntropyPercent}%, ShouldReset: {shouldResetOnNextChange}, IgnoreNext: {ignoreNextChanges}");
            int oldValue = EntropyPercent;
            bool wasReset = false;
            
            if (shouldResetOnNextChange)
            {
                Debug.Log("[EntropyService] Resetting entropy to 0% due to shouldResetOnNextChange flag");
                EntropyPercent = 0;
                shouldResetOnNextChange = false;
                wasReset = true;
                ignoreNextChanges = CHANGES_TO_IGNORE;
                Debug.Log($"[EntropyService] Will ignore next {CHANGES_TO_IGNORE} entropy changes");
            }
            else if (ignoreNextChanges > 0)
            {
                Debug.Log($"[EntropyService] Ignoring entropy change ({ignoreNextChanges} more to ignore)");
                ignoreNextChanges--;
                return;
            }
            else
            {
                EntropyPercent = Mathf.Max(0, EntropyPercent - amount);
            }
            
            if (oldValue != EntropyPercent)
            {
                Debug.Log($"[EntropyService] Entropy changed from {oldValue}% to {EntropyPercent}% (WasReset: {wasReset})");
                OnEntropyChanged?.Invoke(EntropyPercent, wasReset);
            }
        }

        public void ModifyEntropy(float amount)
        {
            Debug.Log($"[EntropyService] ModifyEntropy called with amount: {amount}. Current: {EntropyPercent}%, ShouldReset: {shouldResetOnNextChange}, IgnoreNext: {ignoreNextChanges}");
            int oldValue = EntropyPercent;
            bool wasReset = false;
            
            if (shouldResetOnNextChange)
            {
                Debug.Log("[EntropyService] Resetting entropy to 0% due to shouldResetOnNextChange flag");
                EntropyPercent = 0;
                shouldResetOnNextChange = false;
                wasReset = true;
                ignoreNextChanges = CHANGES_TO_IGNORE;
                Debug.Log($"[EntropyService] Will ignore next {CHANGES_TO_IGNORE} entropy changes");
            }
            else if (ignoreNextChanges > 0)
            {
                Debug.Log($"[EntropyService] Ignoring entropy change ({ignoreNextChanges} more to ignore)");
                ignoreNextChanges--;
                return;
            }
            else
            {
                // Don't multiply by 100 if it's a reduction (negative amount)
                int change = amount < 0 ? Mathf.RoundToInt(amount) : Mathf.RoundToInt(amount * 100);
                int newValue = EntropyPercent + change;
                
                if (newValue >= 100)
                {
                    Debug.Log("[EntropyService] Entropy would exceed 100%, setting to 100% and enabling reset flag");
                    EntropyPercent = 100;
                    shouldResetOnNextChange = true;
                }
                else
                {
                    EntropyPercent = Mathf.Clamp(newValue, 0, 100);
                }
            }
            
            if (oldValue != EntropyPercent)
            {
                Debug.Log($"[EntropyService] Entropy changed from {oldValue}% to {EntropyPercent}% (WasReset: {wasReset})");
                OnEntropyChanged?.Invoke(EntropyPercent, wasReset);
            }
        }
    }
}