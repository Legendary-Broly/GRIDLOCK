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
        private string selectedTool;

        public event Action OnToolUsed;
        public event Action OnPivotActivated;
        public event Action OnPivotDeactivated;

        public SymbolToolService(IGridService gridService, IVirusService virusService)
        {
            this.gridService = gridService ?? throw new ArgumentNullException(nameof(gridService));
            this.virusService = virusService ?? throw new ArgumentNullException(nameof(virusService));
        }

        public bool IsPivotActive() => isPivotActive;

        public string GetSelectedTool() => selectedTool;

        public void SetSelectedTool(string tool)
        {
            if (string.IsNullOrEmpty(tool))
                throw new ArgumentException("Tool cannot be null or empty", nameof(tool));

            selectedTool = tool;
        }

        public void UsePurgeTool(int x, int y)
        {
            if (!CanUseToolAt(x, y, ToolConstants.PURGE_TOOL)) return;

            if (gridService.GetSymbolAt(x, y) == ToolConstants.VIRUS_SYMBOL)
            {
                gridService.SetSymbol(x, y, "");
                virusService.RemoveVirus(x, y);

                if (gridService.IsFlaggedAsVirus(x, y))
                {
                    gridService.SetVirusFlag(x, y, false);
                }
            }
            
            gridService.RevealTile(x, y, true);
            OnToolUsed?.Invoke();
        }

        public void UseForkTool(int x, int y)
        {
            if (!CanUseToolAt(x, y, ToolConstants.FORK_TOOL)) return;

            if (gridService.IsFlaggedAsVirus(x, y))
            {
                gridService.SetVirusFlag(x, y, false);
            }

            gridService.SetLastRevealedTile(new Vector2Int(x, y));
            OnToolUsed?.Invoke();
        }

        public void UsePivotTool()
        {
            if (!CanUseToolAt(0, 0, ToolConstants.PIVOT_TOOL)) return;

            isPivotActive = true;
            OnPivotActivated?.Invoke();
            OnToolUsed?.Invoke();
            gridService.TriggerGridUpdate();
        }

        public void DeactivatePivot()
        {
            if (!isPivotActive) return;
            
            isPivotActive = false;
            OnPivotDeactivated?.Invoke();
            gridService.TriggerGridUpdate();
        }

        public bool TryPlaceSymbol(int x, int y, string symbol)
        {
            if (!IsValidToolPlacement(x, y, symbol)) return false;

            switch (symbol)
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
                default:
                    return false;
            }

            return true;
        }

        public bool CanUseToolAt(int x, int y, string tool)
        {
            if (!gridService.IsInBounds(x, y)) return false;

            switch (tool)
            {
                case ToolConstants.PURGE_TOOL:
                    return IsValidTargetTile(x, y);
                case ToolConstants.FORK_TOOL:
                    return gridService.IsTileRevealed(x, y);
                case ToolConstants.PIVOT_TOOL:
                    return true;
                default:
                    return false;
            }
        }

        public bool IsValidToolPlacement(int x, int y, string tool)
        {
            if (string.IsNullOrEmpty(tool)) return false;

            switch (tool)
            {
                case ToolConstants.PURGE_TOOL:
                    return CanUseToolAt(x, y, tool);
                case ToolConstants.FORK_TOOL:
                    return CanUseToolAt(x, y, tool);
                case ToolConstants.PIVOT_TOOL:
                    return CanUseToolAt(x, y, tool);
                default:
                    return false;
            }
        }

        private bool IsValidTargetTile(int x, int y)
        {
            var lastRevealed = gridService.GetLastRevealedTile();
            if (!lastRevealed.HasValue) return false;

            if (isPivotActive)
            {
                return Mathf.Abs(x - lastRevealed.Value.x) == 1 && 
                       Mathf.Abs(y - lastRevealed.Value.y) == 1;
            }

            return Mathf.Abs(x - lastRevealed.Value.x) + 
                   Mathf.Abs(y - lastRevealed.Value.y) == 1;
        }
    }
} 