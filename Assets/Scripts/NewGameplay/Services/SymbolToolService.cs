using UnityEngine;
using System;
using System.Collections.Generic;
using NewGameplay.Interfaces;

namespace NewGameplay.Services
{
    public class SymbolToolService : ISymbolToolService   
    {
        private readonly IGridService gridService;
        private readonly IVirusService virusService;
        private bool isPivotActive = false;
        private Vector2Int? lastRevealedTile = null;

        public event Action OnToolUsed;
        public event Action OnPivotActivated;
        public event Action OnPivotDeactivated;

        public SymbolToolService(IGridService gridService, IVirusService virusService)
        {
            this.gridService = gridService;
            this.virusService = virusService;
        }

        public void UsePurgeTool(int x, int y)
        {
            if (!gridService.IsInBounds(x, y)) return;
            
            // Check if tile is adjacent to last revealed tile
            if (!IsValidTargetTile(x, y)) return;

            // If tile has virus, remove it
            if (gridService.GetSymbolAt(x, y) == ToolConstants.VIRUS_SYMBOL)
            {
                gridService.SetSymbol(x, y, "");
                virusService.RemoveVirus(x, y);

                // Remove virus flag if present
                if (gridService.IsFlaggedAsVirus(x, y))
                {
                    gridService.SetVirusFlag(x, y, false);
                }
            }
            
            // Reveal the tile
            gridService.RevealTile(x, y, true);
            OnToolUsed?.Invoke();
        }

        public void UseForkTool(int x, int y)
        {
            if (!gridService.IsInBounds(x, y)) return;
            
            // Can only fork to revealed tiles
            if (!gridService.IsTileRevealed(x, y)) return;

            lastRevealedTile = new Vector2Int(x, y);
            gridService.SetLastRevealedTile(new Vector2Int(x, y));
            OnToolUsed?.Invoke();
        }

        public void UsePivotTool()
        {
            isPivotActive = true;
            OnPivotActivated?.Invoke();
            OnToolUsed?.Invoke();
        }

        public void DeactivatePivot()
        {
            if (!isPivotActive) return;
            
            isPivotActive = false;
            OnPivotDeactivated?.Invoke();
        }

        public bool IsPivotActive() => isPivotActive;

        public void TryPlaceSymbol(int x, int y, string tool)
        {
            switch (tool)
            {
                case ToolConstants.PURGE_TOOL:
                    UsePurgeTool(x, y);
                    break;
                case ToolConstants.FORK_TOOL:
                    UseForkTool(x, y);
                    break;
                case ToolConstants.PIVOT_TOOL:
                    UsePivotTool();
                    break;
            }
        }

        private bool IsValidTargetTile(int x, int y)
        {
            var lastRevealed = gridService.GetLastRevealedTile();
            if (!lastRevealed.HasValue) return false;

            // If pivot is active, check diagonal
            if (isPivotActive)
            {
                return Mathf.Abs(x - lastRevealed.Value.x) == 1 && 
                       Mathf.Abs(y - lastRevealed.Value.y) == 1;
            }

            // Otherwise check adjacent
            return Mathf.Abs(x - lastRevealed.Value.x) + 
                   Mathf.Abs(y - lastRevealed.Value.y) == 1;
        }
    }
} 