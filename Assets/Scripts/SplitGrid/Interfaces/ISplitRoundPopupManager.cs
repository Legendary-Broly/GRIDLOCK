using System;
using UnityEngine;

namespace SplitGrid.Interfaces
{
    public interface ISplitRoundPopupManager
    {
        event Action OnRoundStartConfirmed;
        event Action OnRoundCompleteConfirmed;
        
        // Round Transition UI
        void ShowRoundStartPopup(int roundNumber, Action onConfirm);
        void ShowRoundCompletePopup(int roundNumber, Action onConfirm);
        void ShowExtractPopup(Action onConfirm);
        void ShowGameOverPopup(bool isVictory, Action onConfirm);
        
        // UI State Management
        void HideAllPopups();
        bool IsAnyPopupVisible();
        
        // Round Information
        void UpdateRoundInfo(int roundNumber, int totalRounds);
        void UpdateProgressInfo(GridID gridId, int revealedFragments, int requiredFragments);
    }
} 