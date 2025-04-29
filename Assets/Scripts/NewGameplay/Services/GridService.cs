using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NewGameplay.Interfaces;
using NewGameplay.Configuration;
using NewGameplay.Strategies;
using NewGameplay.Utility;
using NewGameplay;
using NewGameplay.Models;

namespace NewGameplay.Services
{
    public class GridService : IGridService
    {
        private readonly IGridStateService gridStateService;
        private readonly ISymbolPlacementService symbolPlacementService;
        private readonly IPurgeEffectService purgeEffectService;
        private readonly IVirusSpreadService virusSpreadService;
        public string GetSymbolAt(int x, int y) => gridStateService.GetSymbolAt(x, y);
        public bool IsTilePlayable(int x, int y) => gridStateService.IsTilePlayable(x, y);
        public bool IsInBounds(int x, int y) => gridStateService.IsInBounds(x, y);
        private TileState[,] tileStates;
        public event Action OnGridUpdated;
        public int GridSize => gridStateService.GridSize;
        public int GridWidth => gridStateService.GridWidth;
        public int GridHeight => gridStateService.GridHeight;
        public string[,] GridState => gridStateService.GridState;
        public bool[,] TilePlayable => gridStateService.TilePlayable;
        public void TryPlaceSymbol(int x, int y, string symbol)
        {
            symbolPlacementService.TryPlaceSymbol(x, y, symbol);
        }

        public GridService(
            IGridStateService gridStateService,
            ISymbolPlacementService symbolPlacementService,
            IPurgeEffectService purgeEffectService,

            IVirusSpreadService virusSpreadService)
        {
            this.gridStateService = gridStateService;
            this.symbolPlacementService = symbolPlacementService;
            this.purgeEffectService = purgeEffectService;

            this.virusSpreadService = virusSpreadService;
            
            // Pass the purge effect service to the virus spread service
            virusSpreadService.SetPurgeEffectService(purgeEffectService);

            // Subscribe to events from specialized services
            gridStateService.OnGridStateChanged += HandleGridStateChanged;
            symbolPlacementService.OnSymbolPlaced += HandleSymbolPlaced;


            virusSpreadService.OnVirusSpread += HandleVirusSpread;
        }
        // Inside GridService.cs
        
        private GridViewNew gridView;
        public void SetGridView(GridViewNew view)
        {
            this.gridView = view;
        }
        public void RefreshTile(int x, int y)
        {
            gridView?.RefreshTileAt(x, y);
        }

        public void InitializeTileStates(int width, int height)
        {
            gridStateService.InitializeTileStates(width, height);
        }

        public void RevealTile(int x, int y)
        {
            if (IsInBounds(x, y))
            {
                gridStateService.SetTileState(x, y, TileState.Revealed);
            }
        }

        public bool IsTileRevealed(int x, int y)
        {
            if (IsInBounds(x, y))
            {
                return gridStateService.GetTileState(x, y) == TileState.Revealed;
            }
            return false;
        }

        public void SetEntropyService(IEntropyService entropyService)
        {
            virusSpreadService.SetEntropyService(entropyService);
        }

        public void TryPlaceSymbol(int x, int y)
        {
            var symbol = InjectServiceLocator.Service.SelectedSymbol;
            if (string.IsNullOrEmpty(symbol)) return;

            // No more adjacency requirement for purge symbols - they can be placed anywhere
            // and will trigger their effect when a virus becomes adjacent

            // If we get here, the placement should succeed
            symbolPlacementService.TryPlaceSymbol(x, y, symbol);
            InjectServiceLocator.Service.ClearSelectedSymbol(symbol);
        }

        public void SetSymbol(int x, int y, string symbol)
        {
            // Check if we're trying to overwrite a Data Fragment
            string existingSymbol = gridStateService.GetSymbolAt(x, y);
            
            // Special case - allow setting to "DF" for data fragment
            if (symbol == "DF")
            {
                gridStateService.SetSymbol(x, y, symbol);
                Debug.Log($"[GridService] SetSymbol called for DF at ({x},{y})");
                return;
            }

            // Regular case - protect DF from being overwritten
            if (existingSymbol == "DF")
            {
                Debug.Log("[GridService] Attempted to overwrite Data Fragment. Action blocked.");
                return;
            }

            // Prevent viruses from overwriting DF
            if (symbol == "X" && existingSymbol == "DF")
            {
                Debug.Log("[GridService] Attempted to place virus on Data Fragment. Action blocked.");
                return;
            }

            // ✅ FIX: Use gridStateService directly — avoid recursive call
            gridStateService.SetSymbol(x, y, symbol);
            Debug.Log($"[GridService] SetSymbol placed symbol '{symbol}' at ({x},{y})");
        }

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
        
        public List<Vector2Int> GetAllEmptyTilePositions()
        {
            var emptyPositions = new List<Vector2Int>();
            
            for (int y = 0; y < gridStateService.GridHeight; y++)
            {
                for (int x = 0; x < gridStateService.GridWidth; x++)
                {
                    if (string.IsNullOrEmpty(gridStateService.GetSymbolAt(x, y)) && gridStateService.IsTilePlayable(x, y))
                    {
                        emptyPositions.Add(new Vector2Int(x, y));
                    }
                }
            }
            
            return emptyPositions;
        }

        public void SetTilePlayable(int x, int y, bool playable)
        {
            gridStateService.SetTilePlayable(x, y, playable);
        }
        public TileState GetTileState(int x, int y)
        {
            if (IsInBounds(x, y))
            {
                return gridStateService.GetTileState(x, y);
            }
            return TileState.Hidden; // Default fallback
        }
    }
}