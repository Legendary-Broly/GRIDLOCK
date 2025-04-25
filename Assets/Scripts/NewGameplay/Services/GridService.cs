using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NewGameplay.Interfaces;

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
        }

        public void SetSymbol(int x, int y, string symbol)
        {
            // Handle symbol placement effects first
            if (symbol == purgeSymbol)
            {
                HandlePurgeEffect(x, y);
                // Ensure the tile is playable after purge effect
                gridState[x, y] = null;
                tilePlayable[x, y] = true;
                // Check for loop transformations after purge effect
                CheckLoopTransformations();
                return; // Exit early since we've handled everything
            }
            else if (symbol == loopSymbol)
            {
                // Store the current symbol to prevent recursion
                string currentSymbol = gridState[x, y];
                gridState[x, y] = symbol;
                HandleLoopEffect(x, y);
                // No need to check for transformations here since we just handled this loop
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
            List<Vector2Int> existingViruses = new();
            List<Vector2Int> emptySpots = new();
            List<Vector2Int> adjacentEmptySpots = new();

            // First, collect all virus positions and empty spots
            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    if (gridState[x, y] == virusSymbol)
                        existingViruses.Add(new Vector2Int(x, y));
                    else if (tilePlayable[x, y])
                        emptySpots.Add(new Vector2Int(x, y));
                }
            }

            // If there are existing viruses, find empty spots adjacent to them
            if (existingViruses.Count > 0)
            {
                Vector2Int[] directions = new[] {
                    new Vector2Int(1, 0), new Vector2Int(-1, 0),
                    new Vector2Int(0, 1), new Vector2Int(0, -1)
                };

                foreach (var virus in existingViruses)
                {
                    foreach (var dir in directions)
                    {
                        var adjacentPos = virus + dir;
                        if (IsInBounds(adjacentPos.x, adjacentPos.y) && 
                            tilePlayable[adjacentPos.x, adjacentPos.y] &&
                            gridState[adjacentPos.x, adjacentPos.y] == null)
                        {
                            adjacentEmptySpots.Add(adjacentPos);
                        }
                    }
                }
            }

            // Determine number of viruses to spawn based on entropy
            int virusesToSpawn = 1; // Default is 1
            if (entropyService != null)
            {
                float entropy = entropyService.CurrentEntropy;
                if (entropy > 0.66f) // 67-100%
                    virusesToSpawn = 3;
                else if (entropy > 0.33f) // 34-66%
                    virusesToSpawn = 2;
                // else keep default of 1 for 0-33%
                
                Debug.Log($"[Virus] Spawning {virusesToSpawn} viruses at {entropy*100:F1}% entropy");
            }

            // Spawn the determined number of viruses
            for (int i = 0; i < virusesToSpawn && (adjacentEmptySpots.Count > 0 || emptySpots.Count > 0); i++)
            {
                Vector2Int spawnPos;

                // 90% chance to spawn adjacent to existing virus if possible
                if (adjacentEmptySpots.Count > 0 && (rng.NextDouble() < 0.9 || emptySpots.Count == 0))
                {
                    int index = rng.Next(adjacentEmptySpots.Count);
                    spawnPos = adjacentEmptySpots[index];
                    adjacentEmptySpots.RemoveAt(index);
                    // Also remove from emptySpots if it exists there
                    emptySpots.Remove(spawnPos);
                    Debug.Log($"[Virus] Spawning virus adjacent to existing at ({spawnPos.x}, {spawnPos.y})");
                }
                else if (emptySpots.Count > 0)
                {
                    int index = rng.Next(emptySpots.Count);
                    spawnPos = emptySpots[index];
                    emptySpots.RemoveAt(index);
                    Debug.Log($"[Virus] Spawning virus at random position ({spawnPos.x}, {spawnPos.y})");
                }
                else
                {
                    Debug.Log("[Virus] No valid spawn positions remaining");
                    break;
                }

                SetSymbol(spawnPos.x, spawnPos.y, virusSymbol);
            }

            // Now handle existing viruses spreading
            foreach (var pos in existingViruses)
            {
                if (IsAdjacentToSymbol(pos.x, pos.y, purgeSymbol)) continue;

                Vector2Int[] directions = new[] {
                    new Vector2Int(1, 0), new Vector2Int(-1, 0),
                    new Vector2Int(0, 1), new Vector2Int(0, -1)
                };

                directions = directions.OrderBy(_ => rng.Next()).ToArray();
                foreach (var dir in directions)
                {
                    var t = pos + dir;
                    if (IsInBounds(t.x, t.y) && tilePlayable[t.x, t.y])
                    {
                        SetSymbol(t.x, t.y, virusSymbol);
                        break;
                    }
                }
            }
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
            List<Vector2Int> purgePositions = new();
            
            // First collect all ∆ positions
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

            // Then process each ∆
            foreach (var pos in purgePositions)
            {
                HandlePurgeEffect(pos.x, pos.y);
            }
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
    }
}