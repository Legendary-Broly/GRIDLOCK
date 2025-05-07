using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NewGameplay.Interfaces;
using NewGameplay.Strategies;
using NewGameplay.Utility;
using NewGameplay;
using NewGameplay.Models;
using NewGameplay.Enums;
using NewGameplay.Controllers;
using NewGameplay.Views;
namespace NewGameplay.Services
{
    public class GridService : IGridService
    {
        private readonly IGridStateService gridStateService;
        private readonly ISymbolPlacementService symbolPlacementService;
        private readonly IPurgeEffectService purgeEffectService;
        private readonly IVirusService virusService;
        //public event Action OnCorrectFlagPlaced; <--- this is the event that will be triggered when the correct flag is placed(WIP)
        private ITileElementService tileElementService;
        private IProgressTrackerService progressService;

        public ITileElementService TileElementService => tileElementService;

        private Vector2Int? lastRevealedTile = null;
        private bool gridInteractionLocked = true;
        private bool firstRevealPermitted = false;
        private bool roundSpawnsCompleted = false;
        private bool canUseVirusFlag = false;
        public void EnableVirusFlag() => canUseVirusFlag = true;
        public void DisableVirusFlag() => canUseVirusFlag = false;
        public bool CanUseVirusFlag() => canUseVirusFlag;

        public event Action OnGridUpdated;
        public int GridWidth => gridStateService.GridWidth;
        public int GridHeight => gridStateService.GridHeight;

        private GridViewNew gridView;
        private GameOverController gameOverController;

        public GridService(
            IGridStateService gridStateService,
            ISymbolPlacementService symbolPlacementService,
            IPurgeEffectService purgeEffectService,
            IVirusService virusService)
        {
            this.gridStateService = gridStateService;
            this.symbolPlacementService = symbolPlacementService;
            this.purgeEffectService = purgeEffectService;
            this.virusService = virusService;

            gridStateService.OnGridStateChanged += HandleGridStateChanged;
            symbolPlacementService.OnSymbolPlaced += HandleSymbolPlaced;
        }

        public void SetGameOverController(GameOverController controller)
        {
            this.gameOverController = controller;
        }

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

        public void LockInteraction()
        {
            gridInteractionLocked = true;
        }

        public void UnlockInteraction()
        {
            Debug.Log("[GridService] Grid interaction unlocked.");
            gridInteractionLocked = false;
        }

        public void InitializeTileStates(int width, int height)
        {
            gridStateService.SetGridSize(width, height);
        }
        
        public void RevealTile(int x, int y, bool forceReveal = false)
        {
            Debug.Log($"[GridService] RevealTile called for ({x},{y}) - forceReveal={forceReveal}");
            Debug.Log($"[GridService] Current state - inBounds={IsInBounds(x, y)}, lastRevealedTile={lastRevealedTile}");

            if (!IsInBounds(x, y))
            {
                Debug.Log($"[GridService] RevealTile failed - !inBounds");
                return;
            }

            var currentState = gridStateService.GetTileState(x, y);
            if (currentState == TileState.Revealed && !forceReveal)
            {
                Debug.Log($"[GridService] RevealTile failed - already revealed");
                return;
            }

            Debug.Log($"[GridService] Proceeding with reveal at ({x},{y})");
            gridStateService.SetTileState(x, y, TileState.Revealed);
            tileElementService?.TriggerElementEffect(x, y);

            if (progressService != null && GetSymbolAt(x, y) == "DF")
            {
                progressService.NotifyFragmentRevealed();
            }

            lastRevealedTile = new Vector2Int(x, y);
            firstRevealPermitted = false;

            // Make adjacent tiles playable
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (var dir in directions)
            {
                int nx = x + dir.x;
                int ny = y + dir.y;
                if (IsInBounds(nx, ny) && !IsTileRevealed(nx, ny))
                {
                    // No need to set playable here
                }
            }

            EnableVirusFlag();
            OnGridUpdated?.Invoke();
            Debug.Log($"[GridService] RevealTile completed - lastRevealedTile now ({lastRevealedTile?.x},{lastRevealedTile?.y})");
        }

        public bool IsTileRevealed(int x, int y)
        {
            return IsInBounds(x, y) && gridStateService.GetTileState(x, y) == TileState.Revealed;
        }

        public bool IsFirstRevealDone()
        {
            for (int y = 0; y < GridHeight; y++)
                for (int x = 0; x < GridWidth; x++)
                    if (GetTileState(x, y) == TileState.Revealed)
                        return true;
            return false;
        }

        private bool IsAdjacentToLastRevealed(int x, int y)
        {
            if (!lastRevealedTile.HasValue) return false;
            Vector2Int last = lastRevealedTile.Value;
            return Mathf.Abs(x - last.x) + Mathf.Abs(y - last.y) == 1;
        }

        public bool CanRevealTile(int x, int y)
        {
            bool inBounds = IsInBounds(x, y);
            bool revealed = IsTileRevealed(x, y);
            bool hasStart = lastRevealedTile.HasValue;
            bool isAdjacent = IsAdjacentToLastRevealed(x, y);

            if (gridInteractionLocked) return false;
            if (!inBounds || revealed) return false;
            if (!hasStart) return firstRevealPermitted;
            return isAdjacent;
        }


        public void TryPlaceSymbol(int x, int y)
        {
            var symbol = InjectServiceLocator.Service?.SelectedSymbol;
            if (string.IsNullOrEmpty(symbol)) return;

            if (CanRevealTile(x, y))
            {
                symbolPlacementService.TryPlaceSymbol(x, y, symbol);
                InjectServiceLocator.Service?.ClearSelectedSymbol();
            }
        }

        public void TryPlaceSymbol(int x, int y, string symbol)
        {
            if (!IsInBounds(x, y) || !CanRevealTile(x, y)) return;

            if (string.IsNullOrEmpty(gridStateService.GetGridState(x, y)))
            {
                gridStateService.SetGridState(x, y, symbol);
                OnGridUpdated?.Invoke();
            }
        }

        public void SetSymbol(int x, int y, string symbol)
        {
            string existingSymbol = gridStateService.GetGridState(x, y);

            if (symbol == "DF")
            {
                gridStateService.SetGridState(x, y, symbol);
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

            gridStateService.SetGridState(x, y, symbol);
        }

        public void ClearAllExceptViruses()
        {
            // Optional: Implement if needed
        }

        public void ClearAllTiles()
        {
            gridStateService.ClearAllTiles();
            lastRevealedTile = null;
        }

        private void HandleGridStateChanged()
        {
            OnGridUpdated?.Invoke();
        }
        public void SetVirusFlag(int x, int y, bool flagged)
        {
            gridStateService.SetVirusFlag(x, y, flagged);
        }

        public bool IsFlaggedAsVirus(int x, int y)
        {
            return gridStateService.IsFlaggedAsVirus(x, y);
        }

        private void HandleSymbolPlaced()
        {
            OnGridUpdated?.Invoke();
        }

        public Vector2Int? GetLastRevealedTile()
        {
            return lastRevealedTile;
        }
        public void SetLastRevealedTile(Vector2Int pos)
        {
            lastRevealedTile = pos;
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
                    if (string.IsNullOrEmpty(gridStateService.GetGridState(x, y)))
                    {
                        emptyPositions.Add(new Vector2Int(x, y));
                    }
                }
            }

            return emptyPositions;
        }

        public void SetTileState(int x, int y, TileState state)
        {
            gridStateService.SetTileState(x, y, state);
        }

        public TileState GetTileState(int x, int y)
        {
            return IsInBounds(x, y) ? gridStateService.GetTileState(x, y) : TileState.Hidden;
        }

        public bool IsInBounds(int x, int y) => x >= 0 && x < GridWidth && y >= 0 && y < GridHeight;

        public void SetProgressService(IProgressTrackerService service)
        {
            this.progressService = service;
        }

        public void SetFirstRevealPermitted(bool value)
        {
            firstRevealPermitted = value;
        }

        public void ResetRoundSpawns()
        {
            roundSpawnsCompleted = false;
        }

        public string GetSymbolAt(int x, int y)
        {
            // Delegate to the underlying state service
            return gridStateService.GetSymbolAt(x, y);
        }
    }
}
