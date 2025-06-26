using System;
using UnityEngine;

namespace NewGameplay.SplitGrid.Interfaces
{
    public interface ISplitProgressTrackerService
    {
        event Action OnProgressChanged;
        
        int FragmentsFound { get; }
        int RequiredFragments { get; }
        
        void SetRequiredFragments(int count);
        bool HasMetGoal();
        void ResetProgress();
        void NotifyFragmentRevealed();
        void NotifyFragmentRevealed(int x, int y);
        int GetRevealedFragmentCount();
        bool AreAllFragmentsRevealed();
        
        // Fragment Management
        void SetRequiredFragments(GridID gridId, int count);
        int GetRequiredFragments(GridID gridId);
        int GetRevealedFragments(GridID gridId);
        bool IsGridComplete(GridID gridId);
        
        // Progress State
        float GetProgressPercentage();
        
        // Grid-specific Progress
        float GetGridProgressPercentage(GridID gridId);
    }
} 