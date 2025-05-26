using System;
using UnityEngine;

namespace SplitGrid.Interfaces
{
    public interface ISplitProgressTrackerService
    {
        event Action OnProgressUpdated;
        
        // Fragment Management
        void SetRequiredFragments(GridID gridId, int count);
        int GetRequiredFragments(GridID gridId);
        int GetRevealedFragments(GridID gridId);
        void NotifyFragmentRevealed(GridID gridId, int x, int y);
        bool AreAllFragmentsRevealed();
        
        // Progress State
        void ResetProgress();
        float GetProgressPercentage();
        
        // Grid-specific Progress
        float GetGridProgressPercentage(GridID gridId);
        bool IsGridComplete(GridID gridId);
    }
} 