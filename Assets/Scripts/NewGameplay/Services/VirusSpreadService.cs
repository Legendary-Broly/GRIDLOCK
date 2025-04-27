using System;
using System.Collections.Generic;
using UnityEngine;
using NewGameplay.Interfaces;
using NewGameplay.Strategies;

namespace NewGameplay.Services
{
    public class VirusSpreadService : IVirusSpreadService
    {
        private readonly IGridStateService gridStateService;
        private IEntropyService entropyService;
        private VirusSpawningStrategy virusSpawningStrategy;
        private IPurgeEffectService purgeEffectService;

        public event Action OnVirusSpread;

        public VirusSpreadService(IGridStateService gridStateService)
        {
            this.gridStateService = gridStateService;
        }

        public void SetEntropyService(IEntropyService entropyService)
        {
            this.entropyService = entropyService;
            virusSpawningStrategy = new VirusSpawningStrategy(
                entropyService,
                new System.Random(),
                gridStateService.GridWidth,
                gridStateService.GridHeight,
                gridStateService.GridState,
                gridStateService.TilePlayable
            );
        }

        public void SetPurgeEffectService(IPurgeEffectService purgeEffectService)
        {
            this.purgeEffectService = purgeEffectService;
        }

        public void SpreadVirus()
        {
            if (entropyService == null)
            {
                Debug.LogError("EntropyService is null! Cannot spread viruses without entropy service.");
                return;
            }

            var spawnPositions = virusSpawningStrategy.GetVirusSpawnPositions();
            foreach (var pos in spawnPositions)
            {
                // Set the tile to virus symbol
                gridStateService.SetSymbol(pos.x, pos.y, "X");
                
                // Mark the tile as not playable
                gridStateService.SetTilePlayable(pos.x, pos.y, false);
            }

            // Check for purge symbols adjacent to newly spawned viruses
            CheckForAdjacentPurgeSymbols(spawnPositions);

            OnVirusSpread?.Invoke();
        }

        private void CheckForAdjacentPurgeSymbols(List<Vector2Int> virusPositions)
        {
            bool shouldProcessPurges = false;
            Vector2Int[] directions = new[] {
                new Vector2Int(1, 0), new Vector2Int(-1, 0),
                new Vector2Int(0, 1), new Vector2Int(0, -1)
            };

            // Check all adjacent tiles to each new virus
            foreach (var virus in virusPositions)
            {
                foreach (var dir in directions)
                {
                    int nx = virus.x + dir.x;
                    int ny = virus.y + dir.y;
                    
                    if (gridStateService.IsInBounds(nx, ny) && gridStateService.GetSymbolAt(nx, ny) == "∆")
                    {
                        // Found a purge symbol adjacent to a virus
                        shouldProcessPurges = true;
                        Debug.Log($"[VirusSpread] Found purge at ({nx},{ny}) adjacent to virus at ({virus.x},{virus.y}), triggering purge effect");
                        break;
                    }
                }
                
                if (shouldProcessPurges)
                    break;
            }

            // If any purge symbols were found, process all purges
            if (shouldProcessPurges && purgeEffectService != null)
            {
                purgeEffectService.ProcessPurges();
            }
        }
    }
} 