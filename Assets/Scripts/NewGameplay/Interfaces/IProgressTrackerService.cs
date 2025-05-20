using System;
using UnityEngine;

namespace NewGameplay.Interfaces
{
    public interface IProgressTrackerService
    {
        event System.Action OnProgressChanged;
        
        int FragmentsFound { get; }
        int RequiredFragments { get; }
        
        void SetRequiredFragments(int count);
        //void IncrementFragmentsFound();
        bool HasMetGoal();
        void ResetProgress();
        void NotifyFragmentRevealed();
        void NotifyFragmentRevealed(int x, int y); 
        int GetRevealedFragmentCount();
    }
}