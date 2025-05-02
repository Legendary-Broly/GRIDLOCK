using System.Collections.Generic;
using UnityEngine;
using NewGameplay.Interfaces;
using NewGameplay.Enums;
using System.Linq;
using NewGameplay.Models;

namespace NewGameplay.Services
{
    public class TileElementService : ITileElementService
    {
        private TileElementType[,] gridElements;
        private readonly int gridWidth;
        private readonly int gridHeight;
        private readonly List<TileElementSO> elementConfigs;
        private readonly IEntropyService entropyService;
        private IGridService gridService;
        private Vector2Int? virusNestPosition = null;

        public int GridWidth => gridWidth;

        public int GridHeight => gridHeight;

        public TileElementService(
            int gridWidth,
            int gridHeight,
            List<TileElementSO> configs,
            IEntropyService entropyService)
        {
            this.gridWidth = gridWidth;
            this.gridHeight = gridHeight;
            this.elementConfigs = configs;
            this.entropyService = entropyService;
            gridElements = new TileElementType[gridWidth, gridHeight];
        }

        public void SetGridService(IGridService service)
        {
            this.gridService = service;
        }

        public void GenerateElements()
        {
            for (int y = 0; y < gridHeight; y++)
                for (int x = 0; x < gridWidth; x++)
                    gridElements[x, y] = TileElementType.Empty;

            List<Vector2Int> positions = new List<Vector2Int>();
            for (int y = 0; y < gridHeight; y++)
                for (int x = 0; x < gridWidth; x++)
                    positions.Add(new Vector2Int(x, y));

            for (int i = positions.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                var temp = positions[i];
                positions[i] = positions[j];
                positions[j] = temp;
            }

            int currentPosition = 0;
            var elementCounts = new Dictionary<TileElementType, int>
            {
                { TileElementType.VirusNest, 1 },
                { TileElementType.EntropyIncreaser, 6 },
                { TileElementType.EntropyReducer, 6 },
                { TileElementType.CodeShard, 18 },
            };

            foreach (var elementCount in elementCounts)
            {
                for (int i = 0; i < elementCount.Value && currentPosition < positions.Count; i++)
                {
                    var pos = positions[currentPosition];
                    gridElements[pos.x, pos.y] = elementCount.Key;

                    if (elementCount.Key == TileElementType.VirusNest)
                    {
                        virusNestPosition = new Vector2Int(pos.x, pos.y);
                    }

                    currentPosition++;
                }
            }
        }

        public void TriggerElementEffect(int x, int y)
        {
            var element = gridElements[x, y];
            var config = elementConfigs.FirstOrDefault(e => e.elementType == element);

            if (config == null)
            {
                Debug.LogWarning($"[TileElementService] No configuration found for element type {element} at ({x},{y})");
                return;
            }

            switch (element)
            {
                case TileElementType.EntropyIncreaser:
                    Debug.Log($"[TileElementService] Increasing entropy by {config.entropyChange}");
                    entropyService.Increase(Mathf.Abs(config.entropyChange));
                    break;

                case TileElementType.EntropyReducer:
                    Debug.Log($"[TileElementService] Reducing entropy by {config.entropyChange}");
                    entropyService.Decrease(Mathf.Abs(config.entropyChange));
                    break;

                case TileElementType.CodeShard:
                    Debug.Log("Code shard found.");
                    break;

                case TileElementType.VirusNest:
                    Debug.Log("[TileElementService] Virus Nest triggered — spawning virus.");
                    gridElements[x, y] = TileElementType.Empty;

                    if (gridService != null)
                    {
                        gridService.SetSymbol(x, y, "X");
                        gridService.SetTilePlayable(x, y, false);
                    }
                    else
                    {
                        Debug.LogError("[TileElementService] GridService not set - cannot spawn virus.");
                    }
                    break;
            }
        }

        public void TriggerElementEffectForFirstVirus()
        {
            if (virusNestPosition.HasValue)
            {
                Vector2Int pos = virusNestPosition.Value;
                Debug.Log($"[TileElementService] Spawning virus from nest at {pos}");
                gridElements[pos.x, pos.y] = TileElementType.Empty;

                if (gridService != null)
                {
                    gridService.SetSymbol(pos.x, pos.y, "X");
                    gridService.SetTilePlayable(pos.x, pos.y, false);
                }
                else
                {
                    Debug.LogError("[TileElementService] GridService not set - cannot spawn virus.");
                }
            }
            else
            {
                Debug.LogWarning("[TileElementService] No virus nest position found.");
            }
        }

        public Vector2Int? GetVirusNestPosition() => virusNestPosition;

        public TileElementType GetElementAt(int x, int y) => gridElements[x, y];

        public TileElementSO GetElementSOAt(int x, int y)
        {
            var type = gridElements[x, y];
            return elementConfigs.FirstOrDefault(e => e.elementType == type);
        }
    }
}
