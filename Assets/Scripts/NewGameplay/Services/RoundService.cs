using NewGameplay;
using NewGameplay.Interfaces;
using NewGameplay.Services;
using NewGameplay.Controllers;
using System.Collections.Generic;
using UnityEngine;
using NewGameplay.Views;
using NewGameplay.ScriptableObjects;


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
        private RoundConfigDatabase roundConfigDatabase;

        public RoundService(
            IGridStateService gridStateService,
            IGridService grid,
            IProgressTrackerService progress,
            IInjectService inject,
            IDataFragmentService dataFragmentService,
            IVirusService virusService,
            ITileElementService tileElementService)
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
                currentRound++;

                // Store the last revealed tile position before resetting
                Vector2Int? lastRevealedTile = null;
                if (grid is GridService gridService)
                {
                    lastRevealedTile = gridService.GetLastRevealedTile();
                }

                var config = roundConfigDatabase.GetConfigForRound(currentRound);
                if (config == null)
                {
                    Debug.LogWarning($"[RoundService] No config found for round {currentRound}, defaulting to 7x7");
                    config = new RoundConfigSO { gridWidth = 7, gridHeight = 7, fragmentRequirement = 1, virusCount = 2 };
                }
                progress.SetRequiredFragments(config.fragmentRequirement);
                tileElementService.ResizeGrid(config.gridWidth, config.gridHeight);

                gridStateService.SetGridSize(config.gridWidth, config.gridHeight);

                grid.ClearAllTiles();

                if (grid is GridService g)
                {
                    g.SetFirstRevealPermitted(false);
                    g.ResetRoundSpawns();
                }

                // Deactivate pivot tool if it's active
                if (grid is GridService gridServiceWithTools && gridServiceWithTools.SymbolToolService != null)
                {
                    gridServiceWithTools.SymbolToolService.DeactivatePivot();
                }

                progress.ResetProgress();

                if (grid is GridService gridService2)
                {
                    gridService2.InitializeTileStates(gridService2.GridWidth, gridService2.GridHeight);
                }

                int fragmentCount = Mathf.Clamp(currentRound, 1, 3); // Up to 3 fragments max
                int virusCount = Mathf.Clamp(currentRound * 2, 2, 12); // Increase over time, capped at 12

                dataFragmentService.SpawnFragments(config.fragmentRequirement);

                if (virusService is VirusService concreteVirusService)
                {
                    concreteVirusService.SpawnViruses(config.virusCount, config.gridWidth, config.gridHeight, lastRevealedTile);
                }

                // Generate elements after viruses are placed
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
                        gridService4.SetLastRevealedTile(pos);
                        gridService4.RevealTile(pos.x, pos.y, true);
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

        public void TriggerRoundReset()
        {
            onRoundReset?.Invoke();
        }

        public int CurrentRound => currentRound;
    }
}