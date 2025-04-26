using System;
using System.Collections.Generic;
using UnityEngine;
using NewGameplay.Interfaces;

namespace NewGameplay.Services
{
    public class LoopEffectService : ILoopEffectService
    {
        private readonly IGridStateService gridStateService;
        private readonly System.Random rng = new();

        public event Action OnLoopTransformed;

        public LoopEffectService(IGridStateService gridStateService)
        {
            this.gridStateService = gridStateService;
        }

        public void CheckLoopTransformations()
        {
            for (int y = 0; y < gridStateService.GridSize; y++)
            {
                for (int x = 0; x < gridStateService.GridSize; x++)
                {
                    if (gridStateService.GetSymbolAt(x, y) == "Θ")
                    {
                        HandleLoopEffect(x, y);
                    }
                }
            }
        }

        public void HandleLoopEffect(int x, int y)
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
                if (gridStateService.IsInBounds(nx, ny) && 
                    !string.IsNullOrEmpty(gridStateService.GetSymbolAt(nx, ny)) && 
                    gridStateService.GetSymbolAt(nx, ny) != "Θ")
                {
                    adjacentTiles.Add(new Vector2Int(nx, ny));
                }
            }

            // If there are adjacent symbols, transform into one of them immediately
            if (adjacentTiles.Count > 0)
            {
                var source = adjacentTiles[rng.Next(adjacentTiles.Count)];
                var symbol = gridStateService.GetSymbolAt(source.x, source.y);
                gridStateService.SetSymbol(x, y, symbol);
                gridStateService.SetTilePlayable(x, y, false);
                Debug.Log($"[Θ] Loop at ({x},{y}) instantly transformed into '{symbol}'");
                OnLoopTransformed?.Invoke();
            }
            else
            {
                // Keep the loop symbol on the grid if no adjacent symbols
                gridStateService.SetSymbol(x, y, "Θ");
                gridStateService.SetTilePlayable(x, y, false);
                Debug.Log($"[Θ] Loop at ({x},{y}) waiting for adjacent symbols");
            }
        }
    }
} 