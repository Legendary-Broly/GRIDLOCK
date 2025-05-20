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
        public event System.Action OnProgressChanged;
        private readonly IDataFragmentService dataFragmentService;
        private readonly List<Vector2Int> revealedFragments = new();
        private readonly IExtractService extractService;

        private int requiredFragments = 1;

        public int FragmentsFound => GetRevealedFragmentCount();
        public int RequiredFragments => requiredFragments;

        public void SetRequiredFragments(int count)
        {
            requiredFragments = count;
            OnProgressChanged?.Invoke();
        }

        public bool HasMetGoal()
        {
            return FragmentsFound >= requiredFragments;
        }

        public void ResetProgress()
        {
            revealedFragments.Clear();
            OnProgressChanged?.Invoke();
        }

        public void NotifyFragmentRevealed()
        {
            OnProgressChanged?.Invoke();
        }

        public void NotifyFragmentRevealed(int x, int y)
        {


            var pos = new Vector2Int(x, y);
            if (!revealedFragments.Contains(pos))
            {
                revealedFragments.Add(pos);
                
                OnProgressChanged?.Invoke();
            }
        }

        public int GetRevealedFragmentCount()
        {
            var count = revealedFragments.Count(pos => dataFragmentService.IsFragmentAt(pos));
            return count;
        }

        public ProgressTrackerService(IDataFragmentService dataFragmentService)
        {
            this.dataFragmentService = dataFragmentService;
        }
    }
}

