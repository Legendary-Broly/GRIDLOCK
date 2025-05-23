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
        private IGridStateService gridStateService;
        private IVirusService virusService;
        private ITileElementService tileElementService;
        private IProgressTrackerService progressService;
        private ISymbolToolService symbolToolService;
        private IChatLogService chatLogService;
        private IDataFragmentService dataFragmentService;
        private Vector2Int? lastRevealedTile = null;
        private bool gridInteractionLocked = true;
        private bool firstRevealPermitted = false;
        private bool roundSpawnsCompleted = false;
        private bool canUseVirusFlag = false;
        public void EnableVirusFlag() => canUseVirusFlag = true;
        public void DisableVirusFlag() => canUseVirusFlag = false;
        public bool CanUseVirusFlag() => canUseVirusFlag;
        private PayloadManager payloadManager;
        public void SetPayloadManager(PayloadManager manager) => payloadManager = manager;

        public event Action OnGridUpdated;
        public int GridWidth => gridStateService.GridWidth;
        public int GridHeight => gridStateService.GridHeight;

        private GridViewNew gridView;

        public ITileElementService TileElementService => tileElementService;

        public GridService(
            IGridStateService gridStateService,
            IVirusService virusService,
            IChatLogService chatLogService, 
            IDataFragmentService dataFragmentService, 
            ITileElementService tileElementService,
            IProgressTrackerService progressService
            )
        {
            this.gridStateService = gridStateService;
            this.virusService = virusService;
            this.chatLogService = chatLogService;
            this.dataFragmentService = dataFragmentService;
            this.tileElementService = tileElementService;
            this.progressService = progressService;

            gridStateService.OnGridStateChanged += HandleGridStateChanged;
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

            gridInteractionLocked = false;
        }

        public void InitializeTileStates(int width, int height)
        {
            gridStateService.SetGridSize(width, height);
        }
        
        public void RevealTile(int x, int y, bool forceReveal = false)
        {

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

            // Check if pivot is active and validate diagonal movement
            if (symbolToolService != null && symbolToolService.IsPivotActive())
            {
                if (!IsValidDiagonalMove(x, y))
                {
                    Debug.Log($"[GridService] RevealTile failed - invalid diagonal move");
                    return;
                }
                symbolToolService.DeactivatePivot();
            }
            else if (!forceReveal && !IsValidAdjacentMove(x, y))
            {
                Debug.Log($"[GridService] RevealTile failed - invalid adjacent move");
                return;
            }

            gridStateService.SetTileState(x, y, TileState.Revealed);
            
            // Remove the duplicate virus check since it's handled in GridInputController
            if (payloadManager != null && payloadManager.ShouldRevealRandomTilesOnVirus() && GetSymbolAt(x, y) == "X")
            {
                var hidden = new List<Vector2Int>();

                for (int gy = 0; gy < GridHeight; gy++)
                {
                    for (int gx = 0; gx < GridWidth; gx++)
                    {
                        if (!IsTileRevealed(gx, gy) && !virusService.HasVirusAt(gx, gy))
                        {
                            hidden.Add(new Vector2Int(gx, gy));
                        }
                    }
                }

                for (int i = 0; i < 2 && hidden.Count > 0; i++)
                {
                    int index = UnityEngine.Random.Range(0, hidden.Count);
                    var pos = hidden[index];
                    hidden.RemoveAt(index);

                    RevealTile(pos.x, pos.y, true);
                    Debug.Log($"[Phishing Payload] Bonus revealed tile at ({pos.x},{pos.y})");
                }
            }

            tileElementService?.TriggerElementEffect(x, y);

            if (progressService != null && dataFragmentService != null && dataFragmentService.IsFragmentAt(new Vector2Int(x, y)))
            {
                chatLogService?.LogDataFragmentReveal();
                progressService.NotifyFragmentRevealed(x, y);
            }

            lastRevealedTile = new Vector2Int(x, y);
            firstRevealPermitted = false;

            EnableVirusFlag();
            OnGridUpdated?.Invoke();

        }

        private bool IsValidDiagonalMove(int x, int y)
        {
            if (!lastRevealedTile.HasValue) return false;
            var last = lastRevealedTile.Value;
            return Mathf.Abs(x - last.x) == 1 && Mathf.Abs(y - last.y) == 1;
        }

        private bool IsValidAdjacentMove(int x, int y)
        {
            if (!lastRevealedTile.HasValue) return firstRevealPermitted;
            var last = lastRevealedTile.Value;
            return Mathf.Abs(x - last.x) + Mathf.Abs(y - last.y) == 1;
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
            bool isDiagonal = false;
            bool isAdjacent = false;
            if (hasStart)
            {
                var last = lastRevealedTile.Value;
                isDiagonal = symbolToolService != null && symbolToolService.IsPivotActive() && Mathf.Abs(last.x - x) == 1 && Mathf.Abs(last.y - y) == 1;
                isAdjacent = (!symbolToolService?.IsPivotActive() ?? true) && Mathf.Abs(last.x - x) + Mathf.Abs(last.y - y) == 1;
            }

            if (gridInteractionLocked) return false;
            if (!inBounds || revealed) return false;
            if (!hasStart) return firstRevealPermitted;
            return isDiagonal || isAdjacent;
        }

        public void TryPlaceSymbol(int x, int y)
        {
            // This method is no longer needed as symbols are now tools
            // Keeping empty implementation for interface compatibility
        }
        public void TryPlaceSymbol(int x, int y, string symbol)
        {
            // This method is no longer needed as symbols are now tools
            // Keeping empty implementation for interface compatibility
        }

        public void SetSymbol(int x, int y, string symbol)
        {
            string existingSymbol = gridStateService.GetGridState(x, y);
            if (existingSymbol != symbol)
            {
                gridStateService.SetGridState(x, y, symbol);
                OnGridUpdated?.Invoke();
            }
        }

        public void ClearAllExceptViruses()
        {
            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    if (GetSymbolAt(x, y) != "X")
                    {
                        SetSymbol(x, y, "");
                    }
                }
            }
            OnGridUpdated?.Invoke();
        }

        public void ClearAllTiles()
        {
            gridStateService.ClearAllTiles(); // ✅ the real clearing now happens here
            OnGridUpdated?.Invoke();
        }


        private void HandleGridStateChanged()
        {
            OnGridUpdated?.Invoke();
        }

        public void SetVirusFlag(int x, int y, bool flagged)
        {
            gridStateService.SetVirusFlag(x, y, flagged);
            OnGridUpdated?.Invoke();
        }

        public bool IsFlaggedAsVirus(int x, int y)
        {
            return gridStateService.IsFlaggedAsVirus(x, y);
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
            OnGridUpdated?.Invoke();
        }

        public List<Vector2Int> GetAllEmptyTilePositions()
        {
            var positions = new List<Vector2Int>();
            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    if (string.IsNullOrEmpty(GetSymbolAt(x, y)))
                    {
                        positions.Add(new Vector2Int(x, y));
                    }
                }
            }
            return positions;
        }

        public List<Vector2Int> GetValidInitialRevealPositions()
        {
            var positions = new List<Vector2Int>();
            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    string symbol = GetSymbolAt(x, y);
                    bool isEmpty = string.IsNullOrEmpty(symbol);
                    bool isVirus = symbol == "X";
                    bool isDataFragment = symbol == "DATA";
                    bool hasElement = tileElementService?.GetElementAt(x, y) != TileElementType.Empty;

                    if (isEmpty && !isVirus && !isDataFragment && !hasElement)
                    {
                        positions.Add(new Vector2Int(x, y));
                    }
                }
            }
            return positions;
        }

        public void SetTileState(int x, int y, TileState state)
        {
            gridStateService.SetTileState(x, y, state);
        }

        public TileState GetTileState(int x, int y)
        {
            return gridStateService.GetTileState(x, y);
        }

        public bool IsInBounds(int x, int y) => x >= 0 && x < GridWidth && y >= 0 && y < GridHeight;

        public void SetProgressService(IProgressTrackerService service)
        {
            this.progressService = service;
        }

        public void SetDataFragmentService(IDataFragmentService service)
        {
            this.dataFragmentService = service;
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
            return gridStateService.GetGridState(x, y);
        }

        public void SetSymbolToolService(ISymbolToolService service)
        {
            this.symbolToolService = service;
        }

        public ISymbolToolService SymbolToolService => symbolToolService;

        public void SetChatLogService(IChatLogService chatLogService)
        {
            this.chatLogService = chatLogService;
        }

        public void SetVirusService(IVirusService service)
        {
            this.virusService = service;
        }

    }
}