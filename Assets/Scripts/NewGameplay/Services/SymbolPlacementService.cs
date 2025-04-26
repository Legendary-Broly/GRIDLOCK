using System;
using UnityEngine;
using NewGameplay.Interfaces;

namespace NewGameplay.Services
{
    public class SymbolPlacementService : ISymbolPlacementService
    {
        private readonly IGridStateService gridStateService;
        private readonly IPurgeEffectService purgeEffectService;
        private readonly ILoopEffectService loopEffectService;

        public event Action OnSymbolPlaced;

        public SymbolPlacementService(
            IGridStateService gridStateService,
            IPurgeEffectService purgeEffectService,
            ILoopEffectService loopEffectService)
        {
            this.gridStateService = gridStateService;
            this.purgeEffectService = purgeEffectService;
            this.loopEffectService = loopEffectService;
        }

        public void TryPlaceSymbol(int x, int y, string symbol)
        {
            if (!gridStateService.IsTilePlayable(x, y)) return;
            if (string.IsNullOrEmpty(symbol)) return;

            // Special check for purge symbol - must be adjacent to a virus
            if (symbol == "∆" && !IsAdjacentToSymbol(x, y, "X"))
            {
                Debug.Log("Purge symbol can only be placed adjacent to a virus");
                return;
            }

            SetSymbol(x, y, symbol);
            
            // Don't make purge symbol tiles unplayable
            if (symbol != "∆")
            {
                gridStateService.SetTilePlayable(x, y, false);
            }

            OnSymbolPlaced?.Invoke();
        }

        private void SetSymbol(int x, int y, string symbol)
        {
            // Handle symbol placement effects first
            if (symbol == "∆")
            {
                gridStateService.SetSymbol(x, y, symbol);
                purgeEffectService.ProcessPurges();
                loopEffectService.CheckLoopTransformations();
                return;
            }
            else if (symbol == "Θ")
            {
                gridStateService.SetSymbol(x, y, symbol);
                loopEffectService.HandleLoopEffect(x, y);
                return;
            }
            
            gridStateService.SetSymbol(x, y, symbol);
            gridStateService.SetTilePlayable(x, y, string.IsNullOrEmpty(symbol));

            if (symbol != "Θ")
            {
                loopEffectService.CheckLoopTransformations();
            }
        }

        public bool IsAdjacentToSymbol(int x, int y, string target)
        {
            Vector2Int[] directions = new[] {
                new Vector2Int(1, 0), new Vector2Int(-1, 0),
                new Vector2Int(0, 1), new Vector2Int(0, -1)
            };

            foreach (var d in directions)
            {
                int cx = x + d.x, cy = y + d.y;
                if (gridStateService.IsInBounds(cx, cy) && gridStateService.GetSymbolAt(cx, cy) == target)
                    return true;
            }
            return false;
        }
    }
} 