using System;
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
                gridStateService.GridSize,
                gridStateService.GridState,
                gridStateService.TilePlayable
            );
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
                gridStateService.SetSymbol(pos.x, pos.y, "X");
            }

            OnVirusSpread?.Invoke();
        }
    }
} 