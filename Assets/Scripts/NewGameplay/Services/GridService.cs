using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NewGameplay.Interfaces;
using NewGameplay.Configuration;
using NewGameplay.Strategies;
using NewGameplay.Utility;
using NewGameplay;

namespace NewGameplay.Services
{
    public class GridService : IGridService
    {
        private readonly IGridStateService gridStateService;
        private readonly ISymbolPlacementService symbolPlacementService;
        private readonly IPurgeEffectService purgeEffectService;
        private readonly ILoopEffectService loopEffectService;
        private readonly IVirusSpreadService virusSpreadService;

        public event Action OnGridUpdated;

        public int GridSize => gridStateService.GridSize;
        public int GridWidth => gridStateService.GridWidth;
        public int GridHeight => gridStateService.GridHeight;
        public string[,] GridState => gridStateService.GridState;
        public bool[,] TilePlayable => gridStateService.TilePlayable;

        public GridService(
            IGridStateService gridStateService,
            ISymbolPlacementService symbolPlacementService,
            IPurgeEffectService purgeEffectService,
            ILoopEffectService loopEffectService,
            IVirusSpreadService virusSpreadService)
        {
            this.gridStateService = gridStateService;
            this.symbolPlacementService = symbolPlacementService;
            this.purgeEffectService = purgeEffectService;
            this.loopEffectService = loopEffectService;
            this.virusSpreadService = virusSpreadService;

            // Subscribe to events from specialized services
            gridStateService.OnGridStateChanged += HandleGridStateChanged;
            symbolPlacementService.OnSymbolPlaced += HandleSymbolPlaced;
            purgeEffectService.OnPurgeProcessed += HandlePurgeProcessed;
            loopEffectService.OnLoopTransformed += HandleLoopTransformed;
            virusSpreadService.OnVirusSpread += HandleVirusSpread;
        }

        public void SetEntropyService(IEntropyService entropyService)
        {
            virusSpreadService.SetEntropyService(entropyService);
        }

        public void TryPlaceSymbol(int x, int y)
        {
            var symbol = InjectServiceLocator.Service.SelectedSymbol;
            if (string.IsNullOrEmpty(symbol)) return;

            // First check if this is a purge symbol that needs to be adjacent to a virus
            if (symbol == "∆" && !symbolPlacementService.IsAdjacentToSymbol(x, y, "X"))
            {
                // Exception: Firewall mutation allows purge symbols to be placed anywhere
                bool firewallActive = false;
                var bootstrapper = UnityEngine.Object.FindFirstObjectByType<NewGameplayBootstrapper>();
                if (bootstrapper != null && bootstrapper.ExposedMutationEffectService != null)
                {
                    firewallActive = bootstrapper.ExposedMutationEffectService.IsMutationActive(MutationType.Firewall);
                }
                
                if (!firewallActive)
                {
                    Debug.Log("[GridService] Purge symbol placement blocked - not adjacent to virus");
                    return; // Return without clearing the symbol
                }
            }

            // If we get here, the placement should succeed
            symbolPlacementService.TryPlaceSymbol(x, y, symbol);
            InjectServiceLocator.Service.ClearSelectedSymbol(symbol);
        }

        public void SetSymbol(int x, int y, string symbol)
        {
            symbolPlacementService.TryPlaceSymbol(x, y, symbol);
        }

        public string GetSymbolAt(int x, int y) => gridStateService.GetSymbolAt(x, y);

        public bool IsTilePlayable(int x, int y) => gridStateService.IsTilePlayable(x, y);

        public void SpreadVirus()
        {
            virusSpreadService.SpreadVirus();
        }

        public void ClearAllExceptViruses()
        {
            gridStateService.ClearAllExceptViruses();
        }

        public void ClearAllTiles()
        {
            gridStateService.ClearAllTiles();
        }

        public void ProcessPurges()
        {
            purgeEffectService.ProcessPurges();
        }

        public void CheckLoopTransformations()
        {
            loopEffectService.CheckLoopTransformations();
        }

        public void EnableRowColumnPurge()
        {
            purgeEffectService.EnableRowColumnPurge();
        }

        public void DisableRowColumnPurge()
        {
            purgeEffectService.DisableRowColumnPurge();
        }

        public bool IsRowColumnPurgeEnabled()
        {
            return purgeEffectService.IsRowColumnPurgeEnabled();
        }

        private void HandleGridStateChanged()
        {
            OnGridUpdated?.Invoke();
        }

        private void HandleSymbolPlaced()
        {
            OnGridUpdated?.Invoke();
        }

        private void HandlePurgeProcessed()
        {
            OnGridUpdated?.Invoke();
        }

        private void HandleLoopTransformed()
        {
            OnGridUpdated?.Invoke();
        }

        private void HandleVirusSpread()
        {
            OnGridUpdated?.Invoke();
        }

        // Public method to trigger grid update event manually
        public void TriggerGridUpdate()
        {
            Debug.Log("[GridService] Manual grid update triggered");
            OnGridUpdated?.Invoke();
        }
    }
}