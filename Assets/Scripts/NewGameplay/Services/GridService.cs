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
using NewGameplay.Enums;

namespace NewGameplay.Services
{
    public class GridService : IGridService
    {
        private readonly IGridStateService gridStateService;
        private readonly ISymbolPlacementService symbolPlacementService;
        private readonly IPurgeEffectService purgeEffectService;
        private readonly IVirusSpreadService virusSpreadService;
        private ITileElementService tileElementService;

        private bool hasSpawnedInitialVirus = false;

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

            virusSpreadService.SetPurgeEffectService(purgeEffectService);

            gridStateService.OnGridStateChanged += HandleGridStateChanged;
            symbolPlacementService.OnSymbolPlaced += HandleSymbolPlaced;
            virusSpreadService.OnVirusSpread += HandleVirusSpread;
        }

        private GridViewNew gridView;
        public void SetGridView(GridViewNew view)
        {
            this.gridView = view;
        }

        public void SetTileElementService(ITileElementService service)
        {
            this.tileElementService = service;
        }

        public void RefreshTile(int x, int y)
        {
            gridView?.RefreshTileAt(x, y);
        }

        public void InitializeTileStates(int width, int height)
        {
            gridStateService.InitializeTileStates(width, height);
        }

        public void RevealTile(int x, int y, bool forceReveal = false)
        {
            if (!IsInBounds(x, y) || IsTileRevealed(x, y))
                return;

            bool isFirstReveal = true;
            for (int yy = 0; yy < GridHeight; yy++)
            {
                for (int xx = 0; xx < GridWidth; xx++)
                {
                    if (GetTileState(xx, yy) == TileState.Revealed)
                    {
                        isFirstReveal = false;
                        break;
                    }
                }
                if (!isFirstReveal) break;
            }

            if (!isFirstReveal && !IsAdjacentToRevealed(x, y) && !forceReveal)
            {
                Debug.Log($"[GridService] Tile ({x},{y}) is not adjacent to any revealed tiles. Reveal blocked.");
                return;
            }

            // --- 🔽 Only reaches this point if reveal is allowed 🔽 ---
            gridStateService.SetTileState(x, y, TileState.Revealed);
            RefreshTile(x, y);

            if (!hasSpawnedInitialVirus)
            {
                hasSpawnedInitialVirus = true;
                tileElementService?.TriggerElementEffectForFirstVirus();
            }

            tileElementService?.TriggerElementEffect(x, y);
            virusSpreadService?.TrySpreadFromExistingViruses();
        }
        public bool IsTileRevealed(int x, int y)
        {
            if (IsInBounds(x, y))
            {
                return gridStateService.GetTileState(x, y) == TileState.Revealed;
            }
            return false;
        }
        public bool IsFirstRevealDone()
        {
            for (int y = 0; y < GridHeight; y++)
                for (int x = 0; x < GridWidth; x++)
                    if (GetTileState(x, y) == TileState.Revealed)
                        return true;
            return false;
        }
        private bool IsAdjacentToRevealed(int x, int y)
        {
            Vector2Int[] directions = new[]
            {
                Vector2Int.up, Vector2Int.down,
                Vector2Int.left, Vector2Int.right
            };

            foreach (var dir in directions)
            {
                int nx = x + dir.x;
                int ny = y + dir.y;

                if (IsInBounds(nx, ny) && IsTileRevealed(nx, ny))
                    return true;
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

            symbolPlacementService.TryPlaceSymbol(x, y, symbol);
            InjectServiceLocator.Service.ClearSelectedSymbol(symbol);
        }

        public void TryPlaceSymbol(int x, int y, string symbol)
        {
            symbolPlacementService.TryPlaceSymbol(x, y, symbol);
        }

        public void SetSymbol(int x, int y, string symbol)
        {
            string existingSymbol = gridStateService.GetSymbolAt(x, y);

            if (symbol == "DF")
            {
                gridStateService.SetSymbol(x, y, symbol);
                Debug.Log($"[GridService] SetSymbol called for DF at ({x},{y})");
                return;
            }

            if (existingSymbol == "DF")
            {
                Debug.Log("[GridService] Attempted to overwrite Data Fragment. Action blocked.");
                return;
            }

            if (symbol == "X" && existingSymbol == "DF")
            {
                Debug.Log("[GridService] Attempted to place virus on Data Fragment. Action blocked.");
                return;
            }

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

        private void HandleVirusSpread()
        {
            OnGridUpdated?.Invoke();
        }

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
            return TileState.Hidden;
        }
        public bool CanRevealTile(int x, int y)
        {
            if (!IsInBounds(x, y) || IsTileRevealed(x, y)) return false;

            // First reveal is always allowed
            for (int yy = 0; yy < GridHeight; yy++)
            {
                for (int xx = 0; xx < GridWidth; xx++)
                {
                    if (GetTileState(xx, yy) == TileState.Revealed)
                        return IsAdjacentToRevealed(x, y);
                }
            }

            return true;
        }

    }
}
