using UnityEngine;
using NewGameplay.Interfaces;
using System;

namespace NewGameplay.Services
{
    public class SymbolPlacementService : ISymbolPlacementService
    {
        private IGridService gridService;
        public event Action OnSymbolPlaced;
        
        public SymbolPlacementService()
        {
        }
        
        public void SetGridService(IGridService gridService)
        {
            this.gridService = gridService;
        }
        
        public void TryPlaceSymbol(int x, int y, string symbol)
        {
            if (gridService == null) return;

            if (symbol == "∆:/run_PURGE.exe")
            {
                if (gridService.GetSymbolAt(x, y) == "X")
                {
                    gridService.SetSymbol(x, y, ""); // Remove the virus
                    Debug.Log($"[SymbolPlacementService] Purge removed virus at ({x},{y})");
                    OnSymbolPlaced?.Invoke();
                }
                else
                {
                    // If not a virus, reveal the tile if not already revealed
                    if (!gridService.IsTileRevealed(x, y))
                    {
                        gridService.RevealTile(x, y, forceReveal: true);
                        Debug.Log($"[SymbolPlacementService] Purge revealed tile at ({x},{y})");
                        OnSymbolPlaced?.Invoke();
                    }
                }
                return;
            }

            // For all other symbols, proceed as normal
            gridService.SetSymbol(x, y, symbol);
            OnSymbolPlaced?.Invoke();
        }
        
        public bool IsAdjacentToSymbol(int x, int y, string targetSymbol)
        {
            if (gridService == null) return false;
            
            // Check adjacent tiles
            Vector2Int[] directions = new[] {
                new Vector2Int(1, 0), new Vector2Int(-1, 0),
                new Vector2Int(0, 1), new Vector2Int(0, -1)
            };

            foreach (var d in directions)
            {
                int nx = x + d.x;
                int ny = y + d.y;
                if (gridService.IsInBounds(nx, ny) && gridService.GetSymbolAt(nx, ny) == targetSymbol)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
