using System;
using UnityEngine;
using NewGameplay.Interfaces;
using NewGameplay.Utility;

namespace NewGameplay.Services
{
    public class SymbolPlacementService : ISymbolPlacementService
    {
        private readonly IGridStateService gridStateService;
        private readonly IPurgeEffectService purgeEffectService;
        private readonly ILoopEffectService loopEffectService;
        private readonly IMutationEffectService mutationEffectService;
        private readonly IEntropyService entropyService;

        public event Action OnSymbolPlaced;

        public SymbolPlacementService(
            IGridStateService gridStateService,
            IPurgeEffectService purgeEffectService,
            ILoopEffectService loopEffectService,
            IMutationEffectService mutationEffectService = null,
            IEntropyService entropyService = null)
        {
            this.gridStateService = gridStateService;
            this.purgeEffectService = purgeEffectService;
            this.loopEffectService = loopEffectService;
            this.mutationEffectService = mutationEffectService;
            this.entropyService = entropyService;
        }

        public void TryPlaceSymbol(int x, int y, string symbol)
        {
            if (!gridStateService.IsTilePlayable(x, y)) return;
            if (string.IsNullOrEmpty(symbol)) return;

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
                
                // Only trigger the purge effect immediately if it's adjacent to a virus
                if (IsAdjacentToSymbol(x, y, "X"))
                {
                    purgeEffectService.ProcessPurges();
                }
                
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
            
            // Check for Σ symbol placement (only applies to Σ with Fear of Change mutation)
            if (symbol == "Σ" && mutationEffectService != null && entropyService != null && 
                mutationEffectService.IsMutationActive(MutationType.FearOfChange))
            {
                entropyService.Decrease(1);  // Apply reduction immediately on placement
                Debug.Log("[Σ] Stabilizer entropy reduced on placement (Fear of Change active)");
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
                int nx = x + d.x;
                int ny = y + d.y;
                if (gridStateService.IsInBounds(nx, ny) && gridStateService.GetSymbolAt(nx, ny) == target)
                {
                    return true;
                }
            }
            return false;
        }
    }
} 