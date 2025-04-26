using System;
using System.Collections.Generic;
using UnityEngine;
using NewGameplay.Interfaces;

namespace NewGameplay.Services
{
    public class LoopEffectService : ILoopEffectService
    {
        private readonly IGridStateService gridStateService;
        private readonly IMutationEffectService mutationEffectService;
        private readonly System.Random rng = new();
        private bool isProcessingLoops = false;

        public event Action OnLoopTransformed;

        public LoopEffectService(IGridStateService gridStateService, IMutationEffectService mutationEffectService)
        {
            this.gridStateService = gridStateService;
            this.mutationEffectService = mutationEffectService;
        }

        public LoopEffectService(GridStateService gridStateService)
        {
            this.gridStateService = gridStateService;
            this.mutationEffectService = null;
        }

        public void CheckLoopTransformations()
        {
            if (isProcessingLoops) return; // Prevent recursive calls
            isProcessingLoops = true;

            bool changesMade;
            do
            {
                changesMade = false;
                for (int y = 0; y < gridStateService.GridSize; y++)
                {
                    for (int x = 0; x < gridStateService.GridSize; x++)
                    {
                        if (gridStateService.GetSymbolAt(x, y) == "Θ")
                        {
                            if (HandleLoopEffect(x, y))
                            {
                                changesMade = true;
                            }
                        }
                    }
                }
            } while (changesMade); // Continue until no more changes are made

            isProcessingLoops = false;
        }

        public bool HandleLoopEffect(int x, int y)
        {
            bool isInfiniteLoop = mutationEffectService?.IsMutationActive(MutationType.InfiniteLoop) ?? false;

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
                if (gridStateService.IsInBounds(nx, ny) && 
                    !string.IsNullOrEmpty(gridStateService.GetSymbolAt(nx, ny)) && 
                    gridStateService.GetSymbolAt(nx, ny) != "Θ")
                {
                    adjacentTiles.Add(new Vector2Int(nx, ny));
                }
            }

            // If there are adjacent symbols, transform based on mutation
            if (adjacentTiles.Count > 0)
            {
                var source = adjacentTiles[rng.Next(adjacentTiles.Count)];
                var symbol = gridStateService.GetSymbolAt(source.x, source.y);

                if (isInfiniteLoop)
                {
                    // Overwrite the entire row and column (including empty tiles)
                    for (int i = 0; i < gridStateService.GridSize; i++)
                    {
                        // Column overwrite (skip other Θ symbols)
                        if (gridStateService.GetSymbolAt(x, i) != "Θ")
                        {
                            gridStateService.SetSymbol(x, i, symbol);
                            gridStateService.SetTilePlayable(x, i, false);
                        }

                        // Row overwrite (skip other Θ symbols)
                        if (gridStateService.GetSymbolAt(i, y) != "Θ")
                        {
                            gridStateService.SetSymbol(i, y, symbol);
                            gridStateService.SetTilePlayable(i, y, false);
                        }
                    }
                    Debug.Log($"[Θ] Infinite Loop at ({x},{y}) overwrote row and column with '{symbol}'");
                }
                else
                {
                    gridStateService.SetSymbol(x, y, symbol);
                    gridStateService.SetTilePlayable(x, y, false);
                    Debug.Log($"[Θ] Loop at ({x},{y}) transformed into '{symbol}'");
                }

                OnLoopTransformed?.Invoke();
                return true; // Indicate that a change was made
            }

            return false; // No changes were made
        }
    }
}
