using UnityEngine;
using NewGameplay.SplitGrid.Interfaces;
using NewGameplay.SplitGrid.Services;
using NewGameplay.SplitGrid.Views;
using NewGameplay.SplitGrid.Data;
using NewGameplay.SplitGrid.Controllers;
using NewGameplay.ScriptableObjects;
using NewGameplay.Interfaces;
using System;

namespace NewGameplay.SplitGrid.Services
{
    public class SplitRoundService : ISplitRoundService
    {
        private readonly ISplitGridService splitGridService;
        private readonly ISplitProgressTrackerService progressTracker;
        private readonly ISplitVirusService virusService;
        private readonly IChatLogService chatLogService;
        private RoundConfigDatabase configDatabase;
        private int currentRound = 0;
        private bool isResetting = false;

        public event Action OnRoundReset;
        public int CurrentRound => currentRound;

        public SplitRoundService(
            ISplitGridService splitGridService,
            ISplitProgressTrackerService progressTracker,
            ISplitVirusService virusService,
            IChatLogService chatLogService)
        {
            this.splitGridService = splitGridService;
            this.progressTracker = progressTracker;
            this.virusService = virusService;
            this.chatLogService = chatLogService;
        }

        public void Initialize(RoundConfigDatabase configDatabase)
        {
            this.configDatabase = configDatabase;
        }

        public int GetGridSizeForRound(int round)
        {
            return Mathf.Clamp(6 + round, 7, 13); // 7 to 13 inclusive
        }

        public void ResetRound()
        {
            if (isResetting) return;
            isResetting = true;

            try
            {
                currentRound++;

                var config = configDatabase.GetConfigForRound(currentRound);
                if (config == null)
                {
                    config = new RoundConfigSO { gridWidth = 7, gridHeight = 7, fragmentRequirement = 1, virusCount = 2 };
                }

                // Configure Grid A
                SetGridDimensions(GridID.GridA, config.gridWidth, config.gridHeight);
                SetVirusCount(GridID.GridA, config.virusCount);
                SetFragmentRequirement(GridID.GridA, config.fragmentRequirement);

                // Configure Grid B
                SetGridDimensions(GridID.GridB, config.gridWidth, config.gridHeight);
                SetVirusCount(GridID.GridB, config.virusCount);
                SetFragmentRequirement(GridID.GridB, config.fragmentRequirement);

                // Clear and initialize grids
                splitGridService.ClearGrid(GridID.GridA);
                splitGridService.ClearGrid(GridID.GridB);
                splitGridService.LockInteraction(GridID.GridA);
                splitGridService.LockInteraction(GridID.GridB);

                // Spawn viruses
                virusService.SpawnViruses(config.virusCount, config.virusCount);
                
                OnRoundReset?.Invoke();
            }
            finally
            {
                isResetting = false;
            }
        }

        public void InitializeRound(int round)
        {
            currentRound = round;
            ResetRound();
        }

        public void CompleteRound()
        {
            // Handle round completion
            splitGridService.UnlockInteraction(GridID.GridA);
            splitGridService.UnlockInteraction(GridID.GridB);
        }

        public void SetGridDimensions(GridID gridId, int width, int height)
        {
            splitGridService.SetGridSize(gridId, width, height);
        }

        public void SetVirusCount(GridID gridId, int count)
        {
            // Virus count is handled during spawn
        }

        public void SetFragmentRequirement(GridID gridId, int count)
        {
            progressTracker.SetRequiredFragments(gridId, count);
        }

        public void NotifyFragmentRevealed(GridID gridId, int x, int y)
        {
            progressTracker.NotifyFragmentRevealed(x, y);
        }

        public void NotifyVirusRevealed(GridID gridId, int x, int y)
        {
            virusService.OnVirusRevealed(gridId, x, y);
        }

        public bool CanExtract(GridID gridId)
        {
            return progressTracker.IsGridComplete(gridId) && 
                   !virusService.AreGridsVirusFree();
        }
    }
} 