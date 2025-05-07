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
        
        private int requiredFragments = 1;

        public int FragmentsFound => dataFragmentService.GetRevealedFragmentCount();
        public int RequiredFragments => requiredFragments;

        public void SetRequiredFragments(int count)
        {
            Debug.Log($"[ProgressTracker] Setting required fragments to {count}");
            requiredFragments = count;
            Debug.Log($"[ProgressTracker] Progress reset to 0/{requiredFragments}");
            OnProgressChanged?.Invoke();
        }

        public bool HasMetGoal()
        {
            return FragmentsFound >= requiredFragments;
        }

        public void ResetProgress()
        {
            Debug.Log("[ProgressTracker] Resetting progress");
            OnProgressChanged?.Invoke();
        }

        public void NotifyFragmentRevealed()
        {
            OnProgressChanged?.Invoke();
        }

        public ProgressTrackerService(IDataFragmentService dataFragmentService)
        {
            this.dataFragmentService = dataFragmentService;
        }
    }
}

