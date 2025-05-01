using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using NewGameplay.Interfaces;
using NewGameplay.Services;
using NewGameplay;

namespace NewGameplay.Services
{
    public class ProgressTrackerService : IProgressTrackerService
    {
        public event System.Action OnProgressGoalReached;
        public event System.Action OnProgressChanged;
        private readonly IDataFragmentService dataFragmentService;
        
        private int foundFragments = 0;
        private int requiredFragments = 1;

        public int FragmentsFound => foundFragments;
        public int RequiredFragments => requiredFragments;

        public void SetRequiredFragments(int count)
        {
            Debug.Log($"[ProgressTracker] Setting required fragments to {count} (will be clamped to 1-3)");
            requiredFragments = Mathf.Clamp(count, 1, 3);
            foundFragments = 0;
            Debug.Log($"[ProgressTracker] Progress reset to 0/{requiredFragments}");
            OnProgressChanged?.Invoke();
        }

        public void IncrementFragmentsFound()
        {
            foundFragments++;
            Debug.Log($"[ProgressTracker] Found fragments incremented to {foundFragments}/{requiredFragments}");
            OnProgressChanged?.Invoke();
            if (HasMetGoal())
            {
                OnProgressGoalReached?.Invoke();
            }
        }

        public bool HasMetGoal()
        {
            return foundFragments >= requiredFragments;
        }

        public void ResetProgress()
        {
            Debug.Log("[ProgressTracker] Resetting progress");
            foundFragments = 0;
            OnProgressChanged?.Invoke();
        }

        public ProgressTrackerService(IDataFragmentService dataFragmentService)
        {
            this.dataFragmentService = dataFragmentService;
        }
    }
}

