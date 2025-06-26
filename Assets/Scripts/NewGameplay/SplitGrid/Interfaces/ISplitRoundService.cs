using System;
using UnityEngine;
using NewGameplay.ScriptableObjects;

namespace NewGameplay.SplitGrid.Interfaces
{
    public interface ISplitRoundService
    {
        event Action OnRoundReset;
        
        int CurrentRound { get; }
        
        // Round Configuration
        void Initialize(RoundConfigDatabase configDatabase);
        int GetGridSizeForRound(int round);
        
        // Round State Management
        void ResetRound();
        void InitializeRound(int round);
        void CompleteRound();
        
        // Grid Configuration
        void SetGridDimensions(GridID gridId, int width, int height);
        void SetVirusCount(GridID gridId, int count);
        void SetFragmentRequirement(GridID gridId, int count);
        
        // Round Progress
        void NotifyFragmentRevealed(GridID gridId, int x, int y);
        void NotifyVirusRevealed(GridID gridId, int x, int y);
        bool CanExtract(GridID gridId);
    }
} 