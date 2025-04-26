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
        private readonly IGridStateService gridStateService;
        private readonly ISymbolPlacementService symbolPlacementService;
        private readonly IPurgeEffectService purgeEffectService;
        private readonly ILoopEffectService loopEffectService;
        private readonly IVirusSpreadService virusSpreadService;

        public event Action OnGridUpdated;

        public int GridSize => gridStateService.GridSize;
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
    }
}