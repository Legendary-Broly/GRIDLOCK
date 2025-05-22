using NewGameplay;
using NewGameplay.Interfaces;
using NewGameplay.Services;
using NewGameplay.Controllers;
using System.Collections.Generic;
using UnityEngine;
using NewGameplay.Views;
using NewGameplay.ScriptableObjects;
using NewGameplay.Data;


namespace NewGameplay.Services
{
    public class RoundService : IRoundService
    {
        private readonly IGridStateService gridStateService;
        private readonly IGridService grid;
        private readonly IProgressTrackerService progress;
        private readonly IInjectService inject;
        private readonly IDataFragmentService dataFragmentService;
        private readonly IVirusService virusService;
        private readonly ITileElementService tileElementService;
        public event System.Action onRoundReset;
        private bool isResetting = false;
        private int currentRound = 0;
        private bool isFirstReset = true;
        private RoundConfigDatabase roundConfigDatabase;
        public RoundConfigSO RoundConfig => roundConfigDatabase.GetConfigForRound(currentRound);



        public RoundService(
            IGridStateService gridStateService,
            IGridService grid,
            IProgressTrackerService progress,
            IInjectService inject,
            IDataFragmentService dataFragmentService,
            IVirusService virusService,
            ITileElementService tileElementService
            )
        {
            this.gridStateService = gridStateService;
            this.grid = grid;
            this.progress = progress;
            this.inject = inject;
            this.dataFragmentService = dataFragmentService;
            this.virusService = virusService;
            this.tileElementService = tileElementService;

        }
        public void Initialize(RoundConfigDatabase configDatabase)
        {
            roundConfigDatabase = configDatabase;
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
                if (!isFirstReset)
                {
                    currentRound++;
                }
                isFirstReset = false;

                // Store the last revealed tile position before resetting
                Vector2Int? lastRevealedTile = null;
                if (grid is GridService gridService)
                {
                    lastRevealedTile = gridService.GetLastRevealedTile();
                }

                var config = roundConfigDatabase.GetConfigForRound(currentRound);
                
                if (config == null)
                {
                    config = new RoundConfigSO { gridWidth = 7, gridHeight = 7, fragmentRequirement = 1, virusCount = 2 };
                }

                if (config.useSplitGrid)
                {
                    Debug.Log($"[RoundService] Split grid round detected — skipping single-grid logic.");
                    // Still trigger the event for other systems that need to know about the round change
                    onRoundReset?.Invoke();
                    return;
                }

                // === SINGLE GRID ROUND SETUP ===
                grid.ClearAllTiles();
                if (grid is GridService g)
                {
                    g.SetFirstRevealPermitted(false); 
                    g.ResetRoundSpawns();
                    g.InitializeTileStates(config.gridWidth, config.gridHeight);
                }

                tileElementService.ResizeGrid(config.gridWidth, config.gridHeight);
                gridStateService.SetGridSize(config.gridWidth, config.gridHeight);
                gridStateService.RestoreEchoTiles();

                // Deactivate pivot tool if it's active
                if (grid is GridService gridServiceWithTools && gridServiceWithTools.SymbolToolService != null)
                {
                    gridServiceWithTools.SymbolToolService.DeactivatePivot();
                }

                progress.SetRequiredFragments(config.fragmentRequirement);
                progress.ResetProgress();

                dataFragmentService.SpawnFragments(config.fragmentRequirement);

                if (virusService is VirusService concreteVirusService)
                {
                    concreteVirusService.SpawnViruses(config.virusCount, config.gridWidth, config.gridHeight, lastRevealedTile);
                }

                if (grid is GridService gridService3)
                {
                    gridService3.TileElementService?.GenerateElements();
                }

                // Restore the last revealed tile if it's within the new grid bounds
                if (lastRevealedTile.HasValue && grid is GridService gridService4)
                {
                    var pos = lastRevealedTile.Value;
                    if (pos.x < config.gridWidth && pos.y < config.gridHeight)
                    {
                        gridService4.RevealTile(pos.x, pos.y, true);
                        gridService4.SetLastRevealedTile(pos);
                        gridService4.SetFirstRevealPermitted(false);
                        gridService4.EnableVirusFlag();
                    }
                }

                grid.TriggerGridUpdate();
                onRoundReset?.Invoke();
                grid.LockInteraction();
            }
            finally
            {
                isResetting = false;
            }
        }


        //public void TriggerRoundReset()
        //{
        //    onRoundReset?.Invoke();
        //}

        public int CurrentRound => currentRound;
    }
}