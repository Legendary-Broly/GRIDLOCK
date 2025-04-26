using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NewGameplay.Interfaces;
using NewGameplay.Configuration;
using NewGameplay.Strategies;

namespace NewGameplay.Services
{
    public class GridService : IGridService
    {
        private readonly int gridSize = 7;
        private readonly string virusSymbol = "X";
        private readonly string purgeSymbol = "∆";
        private readonly string loopSymbol = "Θ";
        private readonly string stabilizerSymbol = "Σ";
        private readonly string surgeSymbol = "Ψ";

        private readonly string[,] gridState;
        private readonly bool[,] tilePlayable;
        private readonly System.Random rng = new();
        private IEntropyService entropyService;
        private VirusSpawningStrategy virusSpawningStrategy;

        public event Action OnGridUpdated;

        public int GridSize => gridSize;

        public string[,] GridState => gridState;

        public bool[,] TilePlayable => tilePlayable;

        public GridService()
        {
            gridState = new string[gridSize, gridSize];
            tilePlayable = new bool[gridSize, gridSize];

            for (int y = 0; y < gridSize; y++)
                for (int x = 0; x < gridSize; x++)
                    tilePlayable[x, y] = true;
        }

        public void SetEntropyService(IEntropyService entropyService)
        {
            this.entropyService = entropyService;
            virusSpawningStrategy = new VirusSpawningStrategy(
                entropyService,
                rng,
                gridSize,
                gridState,
                tilePlayable
            );
        }

        public void TryPlaceSymbol(int x, int y)
        {
            if (!tilePlayable[x, y]) return;

            var symbol = InjectServiceLocator.Service.SelectedSymbol;
            if (string.IsNullOrEmpty(symbol)) return;

            // Special check for purge symbol - must be adjacent to a virus
            if (symbol == purgeSymbol && !IsAdjacentToSymbol(x, y, virusSymbol))
            {
                Debug.Log("Purge symbol can only be placed adjacent to a virus");
                return;
            }

            SetSymbol(x, y, symbol);
            
            // Don't make purge symbol tiles unplayable
            if (symbol != purgeSymbol)
            {
                tilePlayable[x, y] = false;
            }

            InjectServiceLocator.Service.ClearSelectedSymbol(symbol);

            // Check if any loop symbols should transform after placing a new symbol
            CheckLoopTransformations();
            
            // Notify listeners of grid update
            OnGridUpdated?.Invoke();
        }

        public void SetSymbol(int x, int y, string symbol)
        {
            // Handle symbol placement effects first
            if (symbol == purgeSymbol)
            {
                // Place the purge symbol first
                gridState[x, y] = purgeSymbol;
                // Process all purges (this will handle both regular and row/column purges)
                ProcessPurges();
                // Check for loop transformations after purge effect
                CheckLoopTransformations();
                OnGridUpdated?.Invoke();
                return; // Exit early since we've handled everything
            }
            else if (symbol == loopSymbol)
            {
                // Store the current symbol to prevent recursion
                string currentSymbol = gridState[x, y];
                gridState[x, y] = symbol;
                HandleLoopEffect(x, y);
                // No need to check for transformations here since we just handled this loop
                OnGridUpdated?.Invoke();
                return;
            }
            else if (symbol == stabilizerSymbol)
            {
                HandleStabilizerEffect();
            }
            
            // Update grid state and playability
            gridState[x, y] = symbol;
            tilePlayable[x, y] = string.IsNullOrEmpty(symbol);

            // Check for loop transformations after setting any non-loop symbol
            if (symbol != loopSymbol)
            {
                CheckLoopTransformations();
            }
            
            // Notify listeners of grid update
            OnGridUpdated?.Invoke();
        }

        private bool HandlePurgeEffect(int x, int y)
        {
            Debug.Log($"[∆] Starting purge effect at ({x},{y})");
            int purgedVirusCount = 0;
            Vector2Int[] directions = new[] {
                new Vector2Int(1, 0), new Vector2Int(-1, 0),
                new Vector2Int(0, 1), new Vector2Int(0, -1)
            };

            // Check for adjacent viruses
            foreach (var dir in directions)
            {
                int nx = x + dir.x;
                int ny = y + dir.y;
                if (IsInBounds(nx, ny) && gridState[nx, ny] == virusSymbol)
                {
                    Debug.Log($"[∆] Found virus at ({nx},{ny})");
                    gridState[nx, ny] = null;
                    tilePlayable[nx, ny] = true;
                    purgedVirusCount++;
                }
            }

            Debug.Log($"[∆] Total viruses purged: {purgedVirusCount}");

            // Only clear the purge symbol and make its tile playable if we purged a virus
            if (purgedVirusCount > 0)
            {
                gridState[x, y] = null;
                tilePlayable[x, y] = true;
                
                // Reduce entropy based on number of viruses purged
                if (entropyService != null)
                {
                    float entropyReduction = 1 + purgedVirusCount;
                    Debug.Log($"[∆] Attempting to reduce entropy by {entropyReduction}% (base 1 + {purgedVirusCount} viruses)");
                    entropyService.ModifyEntropy(-entropyReduction);
                }
                else
                {
                    Debug.LogError("[∆] EntropyService is null! Cannot reduce entropy");
                }
            }
            else
            {
                Debug.Log($"[∆] No viruses purged, keeping purge symbol at ({x},{y})");
                // Keep the purge symbol on the grid
                gridState[x, y] = purgeSymbol;
                tilePlayable[x, y] = false;
            }

            return purgedVirusCount > 0;
        }

        private void HandleLoopEffect(int x, int y)
        {
            List<Vector2Int> adjacentTiles = new();
            Vector2Int[] directions = new[] {
                new Vector2Int(1, 0), new Vector2Int(-1, 0),
                new Vector2Int(0, 1), new Vector2Int(0, -1)
            };

            // Find all adjacent non-loop symbols
            foreach (var dir in directions)
            {
                int nx = x + dir.x;
                int ny = y + dir.y;
                if (IsInBounds(nx, ny) && !string.IsNullOrEmpty(gridState[nx, ny]) && gridState[nx, ny] != loopSymbol)
                {
                    adjacentTiles.Add(new Vector2Int(nx, ny));
                }
            }

            // If there are adjacent symbols, transform into one of them immediately
            if (adjacentTiles.Count > 0)
            {
                var source = adjacentTiles[rng.Next(adjacentTiles.Count)];
                var symbol = gridState[source.x, source.y];
                gridState[x, y] = symbol; // Replace Θ with the duplicated symbol
                tilePlayable[x, y] = false;
                Debug.Log($"[Θ] Loop at ({x},{y}) instantly transformed into '{symbol}'");

                // Find and update the GridView to refresh the display
                var gridView = UnityEngine.Object.FindFirstObjectByType<GridView>();
                if (gridView != null)
                {
                    gridView.RefreshGrid(this);
                }
            }
            else
            {
                // Keep the loop symbol on the grid if no adjacent symbols
                gridState[x, y] = loopSymbol;
                tilePlayable[x, y] = false;
                Debug.Log($"[Θ] Loop at ({x},{y}) waiting for adjacent symbols");
            }
        }

        private void HandleStabilizerEffect()
        {
            if (entropyService != null)
            {
                entropyService.ModifyEntropy(-0.03f); // Reduce entropy by 3%
            }
        }

        public string GetSymbolAt(int x, int y)
        {
            string symbol = gridState[x, y];
            return symbol;
        }

        public bool IsTilePlayable(int x, int y) => tilePlayable[x, y];

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
                SetSymbol(pos.x, pos.y, VirusConfiguration.VIRUS_SYMBOL);
            }

            OnGridUpdated?.Invoke();
        }

        private bool IsInBounds(int x, int y) => x >= 0 && x < gridSize && y >= 0 && y < gridSize;

        private bool IsAdjacentToSymbol(int x, int y, string target)
        {
            Vector2Int[] directions = new[] {
                new Vector2Int(1, 0), new Vector2Int(-1, 0),
                new Vector2Int(0, 1), new Vector2Int(0, -1)
            };

            foreach (var d in directions)
            {
                int cx = x + d.x, cy = y + d.y;
                if (IsInBounds(cx, cy) && gridState[cx, cy] == target)
                    return true;
            }
            return false;
        }
        public void ClearAllExceptViruses()
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    if (gridState[x, y] != "X")
                    {
                        gridState[x, y] = null;
                        tilePlayable[x, y] = true;
                    }
                }
            }
        }
        public void ClearAllTiles()
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    gridState[x, y] = null;
                    tilePlayable[x, y] = true;
                }
            }
        }

        public void ProcessPurges()
        {
            Debug.Log($"[Purge] rowColumnPurgeEnabled state: {rowColumnPurgeEnabled}");
            List<Vector2Int> purgePositions = new();

            // First collect all Δ positions
            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    if (gridState[x, y] == purgeSymbol)
                    {
                        purgePositions.Add(new Vector2Int(x, y));
                    }
                }
            }

            Debug.Log($"[Purge] Found {purgePositions.Count} purge symbols to process");

            // Then process each Δ
            foreach (var pos in purgePositions)
            {
                bool purgedAny;
                if (rowColumnPurgeEnabled)  // 🔹 Purge+ Mutation active
                {
                    Debug.Log($"[Purge] Using row/column purge at ({pos.x},{pos.y})");
                    purgedAny = PurgeRowAndColumn(pos.x, pos.y);
                }
                else
                {
                    Debug.Log($"[Purge] Using adjacent purge at ({pos.x},{pos.y})");
                    purgedAny = HandlePurgeEffect(pos.x, pos.y);
                }

                // Clean up the purge symbol if it successfully purged anything
                if (purgedAny)
                {
                    gridState[pos.x, pos.y] = null;
                    tilePlayable[pos.x, pos.y] = true;
                }
            }
        }

        private bool PurgeRowAndColumn(int purgeX, int purgeY)
        {
            bool purgedAny = false;
            // 🔹 Purge row
            for (int x = 0; x < gridSize; x++)
            {
                if (gridState[x, purgeY] == virusSymbol)
                {
                    gridState[x, purgeY] = null;  // Remove virus
                    tilePlayable[x, purgeY] = true;  // Make tile playable
                    purgedAny = true;
                }
            }

            // 🔹 Purge column
            for (int y = 0; y < gridSize; y++)
            {
                if (gridState[purgeX, y] == virusSymbol)
                {
                    gridState[purgeX, y] = null;  // Remove virus
                    tilePlayable[purgeX, y] = true;  // Make tile playable
                    purgedAny = true;
                }
            }

            // If we purged any viruses, reduce entropy
            if (purgedAny && entropyService != null)
            {
                // Count how many viruses were purged (we can approximate this as the number of empty tiles in the row + column)
                int purgedCount = 0;
                for (int x = 0; x < gridSize; x++)
                    if (gridState[x, purgeY] == null) purgedCount++;
                for (int y = 0; y < gridSize; y++)
                    if (gridState[purgeX, y] == null) purgedCount++;
                
                float entropyReduction = 1 + purgedCount;
                Debug.Log($"[∆+] Attempting to reduce entropy by {entropyReduction}% (base 1 + {purgedCount} viruses)");
                entropyService.ModifyEntropy(-entropyReduction);
            }

            return purgedAny;
        }

        public void CheckLoopTransformations()
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    if (gridState[x, y] == loopSymbol)
                    {
                        HandleLoopEffect(x, y);
                    }
                }
            }
        }
    
        private bool rowColumnPurgeEnabled = false;

        public void EnableRowColumnPurge()
        {
            rowColumnPurgeEnabled = true;
        }

        public bool IsRowColumnPurgeEnabled()
        {
            return rowColumnPurgeEnabled;
        }
    }
}