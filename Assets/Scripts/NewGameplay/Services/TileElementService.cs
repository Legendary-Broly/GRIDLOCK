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
        private readonly IProgressTrackerService progressService;
        private readonly IEntropyService entropyService;
        private Vector2Int? virusNestPosition = null;

        public TileElementService(
            int gridWidth, 
            int gridHeight, 
            List<TileElementSO> configs,
            IProgressTrackerService progressService,
            IEntropyService entropyService)
        {
            this.gridWidth = gridWidth;
            this.gridHeight = gridHeight;
            this.elementConfigs = configs;
            this.progressService = progressService;
            this.entropyService = entropyService;
            gridElements = new TileElementType[gridWidth, gridHeight];
        }

        public void GenerateElements()
        {
            // Clear grid
            for (int y = 0; y < gridHeight; y++)
                for (int x = 0; x < gridWidth; x++)
                    gridElements[x, y] = TileElementType.Empty;

            // List of all possible positions
            List<UnityEngine.Vector2Int> positions = new List<UnityEngine.Vector2Int>();
            for (int y = 0; y < gridHeight; y++)
                for (int x = 0; x < gridWidth; x++)
                    positions.Add(new UnityEngine.Vector2Int(x, y));

            // Shuffle positions
            for (int i = positions.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                var temp = positions[i];
                positions[i] = positions[j];
                positions[j] = temp;
            }

            int currentPosition = 0;

            // Dictionary to track how many of each type to place
            var elementCounts = new Dictionary<TileElementType, int>
            {
                { TileElementType.VirusNest, 1 },
                { TileElementType.ProgressTile, 7 },
                { TileElementType.EntropyIncreaser, 6 },
                { TileElementType.EntropyReducer, 6 },
                { TileElementType.CodeShardConstructor, 6 },
                { TileElementType.CodeShardArgument, 6 },
                { TileElementType.CodeShardCloser, 6 }
                // Empty tiles will fill the remaining spaces
            };

            // Place each element type according to its count
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
            // The remaining positions will stay Empty (default)
            // Debug log the placement
            Debug.Log($"[TileElementService] Placed elements. Total positions used: {currentPosition}");
            for (int y = 0; y < gridHeight; y++)
            {
                string row = "";
                for (int x = 0; x < gridWidth; x++)
                {
                    row += $"{gridElements[x, y]}, ";
                }
                Debug.Log($"Row {y}: {row}");
            }
        }
        public Vector2Int? GetVirusNestPosition()
        {
            return virusNestPosition;
        }
        public TileElementType GetElementAt(int x, int y) => gridElements[x, y];

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
                case TileElementType.ProgressTile:
                    Debug.Log($"[TileElementService] Triggering progress boost: {config.progressPercent}%");
                    // Convert percentage to actual score (1% = 1 point)
                    int scoreIncrease = Mathf.RoundToInt(config.progressPercent);
                    progressService.ApplyScore(scoreIncrease);
                    break;
                    
                case TileElementType.EntropyIncreaser:
                    Debug.Log($"[TileElementService] Increasing entropy by {config.entropyChange}");
                    entropyService.Increase(Mathf.Abs(config.entropyChange));
                    break;
                    
                case TileElementType.EntropyReducer:
                    Debug.Log($"[TileElementService] Reducing entropy by {config.entropyChange}");
                    entropyService.Decrease(Mathf.Abs(config.entropyChange));
                    break;
                    
                case TileElementType.CodeShardConstructor:
                    Debug.Log("Construct code shard.");
                    break;
                    
                case TileElementType.CodeShardArgument:
                    Debug.Log("Add argument to code shard.");
                    break;
                    
                case TileElementType.CodeShardCloser:
                    Debug.Log("Close code shard.");
                    break;
                    
                case TileElementType.VirusNest:
                    Debug.Log("[TileElementService] Virus Nest triggered — spawning virus.");

                    // Forcefully spawn a virus on this tile
                    gridElements[x, y] = TileElementType.Empty; // Clear the nest after triggering

                    var gridService = Object.FindFirstObjectByType<NewGameplayBootstrapper>()?.ExposedGridService;
                    if (gridService != null)
                    {
                        gridService.SetSymbol(x, y, "X");
                        gridService.SetTilePlayable(x, y, false);
                    }
                    else
                    {
                        Debug.LogError("[TileElementService] Could not find GridService to spawn virus.");
                    }
                    break;
            }
            
            gridElements[x, y] = TileElementType.Empty; // Clear after triggering
        }

        public TileElementSO GetElementSOAt(int x, int y)
        {
            var type = gridElements[x, y];
            return elementConfigs.FirstOrDefault(e => e.elementType == type);
        }
    }
}
