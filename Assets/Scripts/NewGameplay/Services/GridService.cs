using System;
using System.Collections.Generic;
using UnityEngine;
using NewGameplay.Interfaces;
using NewGameplay.Enums;
using NewGameplay.Models;
using NewGameplay.Controllers;

namespace NewGameplay.Services
{
    public class GridService : IGridService
    {
        private readonly IGridStateService gridStateService;
        private readonly IVirusService virusService;
        private ITileElementService tileElementService;
        private IProgressTrackerService progressService;
        private ISymbolToolService symbolToolService;
        private IChatLogService chatLogService;
        private IDataFragmentService dataFragmentService;
        private PayloadManager payloadManager;

        private Vector2Int? lastRevealedTile = null;
        private bool gridInteractionLocked = true;
        private bool firstRevealPermitted = false;
        private bool roundSpawnsCompleted = false;
        private bool canUseVirusFlag = false;

        public event Action OnGridUpdated;

        public int GridWidth => gridStateService.GridWidth;
        public int GridHeight => gridStateService.GridHeight;

        public ITileElementService TileElementService => tileElementService;
        public ISymbolToolService SymbolToolService => symbolToolService;

        public GridService(
            IGridStateService gridStateService,
            IVirusService virusService,
            IChatLogService chatLogService)
        {
            this.gridStateService = gridStateService;
            this.virusService = virusService;
            this.chatLogService = chatLogService;
            gridStateService.OnGridStateChanged += HandleGridStateChanged;
        }

        public void SetTileElementService(ITileElementService service) => tileElementService = service;
        public void SetProgressService(IProgressTrackerService service) => progressService = service;
        public void SetDataFragmentService(IDataFragmentService service) => dataFragmentService = service;
        public void SetSymbolToolService(ISymbolToolService service) => symbolToolService = service;
        public void SetChatLogService(IChatLogService chatLogService) => this.chatLogService = chatLogService;
        public void SetPayloadManager(PayloadManager manager) => payloadManager = manager;

        public void LockInteraction() => gridInteractionLocked = true;
        public void UnlockInteraction() => gridInteractionLocked = false;
        public void EnableVirusFlag() => canUseVirusFlag = true;
        public void DisableVirusFlag() => canUseVirusFlag = false;
        public bool CanUseVirusFlag() => canUseVirusFlag;

        public void InitializeTileStates(int width, int height) => gridStateService.SetGridSize(width, height);

        public void RevealTile(int x, int y, bool forceReveal = false)
        {
            if (!IsInBounds(x, y)) return;

            var currentState = gridStateService.GetTileState(x, y);
            if (currentState == TileState.Revealed && !forceReveal) return;

            if (symbolToolService != null && symbolToolService.IsPivotActive())
            {
                if (!IsValidDiagonalMove(x, y)) return;
                symbolToolService.DeactivatePivot();
            }
            else if (!forceReveal && !IsValidAdjacentMove(x, y))
            {
                return;
            }

            gridStateService.SetTileState(x, y, TileState.Revealed);

            if (payloadManager != null && payloadManager.ShouldRevealRandomTilesOnVirus() && GetSymbolAt(x, y) == "X")
            {
                RevealBonusTiles();
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

        private void RevealBonusTiles()
        {
            var hidden = new List<Vector2Int>();
            for (int y = 0; y < GridHeight; y++)
                for (int x = 0; x < GridWidth; x++)
                    if (!IsTileRevealed(x, y) && !virusService.HasVirusAt(x, y))
                        hidden.Add(new Vector2Int(x, y));

            for (int i = 0; i < 2 && hidden.Count > 0; i++)
            {
                int index = UnityEngine.Random.Range(0, hidden.Count);
                var pos = hidden[index];
                hidden.RemoveAt(index);
                RevealTile(pos.x, pos.y, true);
            }
        }

        public bool IsTileRevealed(int x, int y) => IsInBounds(x, y) && gridStateService.GetTileState(x, y) == TileState.Revealed;
        public bool IsFirstRevealDone()
        {
            for (int y = 0; y < GridHeight; y++)
                for (int x = 0; x < GridWidth; x++)
                    if (GetTileState(x, y) == TileState.Revealed)
                        return true;
            return false;
        }

        public bool CanRevealTile(int x, int y)
        {
            if (gridInteractionLocked || !IsInBounds(x, y) || IsTileRevealed(x, y)) return false;

            if (!lastRevealedTile.HasValue) return firstRevealPermitted;

            var last = lastRevealedTile.Value;
            bool diagonal = symbolToolService?.IsPivotActive() == true && Mathf.Abs(last.x - x) == 1 && Mathf.Abs(last.y - y) == 1;
            bool adjacent = symbolToolService?.IsPivotActive() != true && Mathf.Abs(last.x - x) + Mathf.Abs(last.y - y) == 1;

            return diagonal || adjacent;
        }

        public void SetSymbol(int x, int y, string symbol)
        {
            if (gridStateService.GetGridState(x, y) != symbol)
            {
                gridStateService.SetGridState(x, y, symbol);
                OnGridUpdated?.Invoke();
            }
        }

        public string GetSymbolAt(int x, int y) => gridStateService.GetGridState(x, y);
        public TileState GetTileState(int x, int y) => gridStateService.GetTileState(x, y);
        public void SetTileState(int x, int y, TileState state) => gridStateService.SetTileState(x, y, state);

        public void ClearAllTiles()
        {
            gridStateService.ClearAllTiles();
            OnGridUpdated?.Invoke();
        }

        public void ClearAllExceptViruses()
        {
            for (int y = 0; y < GridHeight; y++)
                for (int x = 0; x < GridWidth; x++)
                    if (GetSymbolAt(x, y) != "X")
                        SetSymbol(x, y, "");
            OnGridUpdated?.Invoke();
        }

        public void SetFirstRevealPermitted(bool value) => firstRevealPermitted = value;

        public Vector2Int? GetLastRevealedTile() => lastRevealedTile;
        public void SetLastRevealedTile(Vector2Int pos) => lastRevealedTile = pos;

        public bool IsFlaggedAsVirus(int x, int y) => gridStateService.IsFlaggedAsVirus(x, y);
        public void SetVirusFlag(int x, int y, bool flagged)
        {
            gridStateService.SetVirusFlag(x, y, flagged);
            OnGridUpdated?.Invoke();
        }

        public List<Vector2Int> GetAllEmptyTilePositions()
        {
            var positions = new List<Vector2Int>();
            for (int y = 0; y < GridHeight; y++)
                for (int x = 0; x < GridWidth; x++)
                    if (string.IsNullOrEmpty(GetSymbolAt(x, y)))
                        positions.Add(new Vector2Int(x, y));
            return positions;
        }

        public List<Vector2Int> GetValidInitialRevealPositions()
        {
            var positions = new List<Vector2Int>();
            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    var symbol = GetSymbolAt(x, y);
                    bool isEmpty = string.IsNullOrEmpty(symbol);
                    bool isVirus = symbol == "X";
                    bool isDataFragment = symbol == "DATA";
                    bool hasElement = tileElementService?.GetElementAt(x, y) != TileElementType.Empty;

                    if (isEmpty && !isVirus && !isDataFragment && !hasElement)
                        positions.Add(new Vector2Int(x, y));
                }
            }
            return positions;
        }

        public void TriggerGridUpdate() => OnGridUpdated?.Invoke();

        public bool IsInBounds(int x, int y) => x >= 0 && x < GridWidth && y >= 0 && y < GridHeight;
        public bool IsValidAdjacentMove(int x, int y) => lastRevealedTile.HasValue && Mathf.Abs(lastRevealedTile.Value.x - x) + Mathf.Abs(lastRevealedTile.Value.y - y) == 1;
        public bool IsValidDiagonalMove(int x, int y) => lastRevealedTile.HasValue && Mathf.Abs(lastRevealedTile.Value.x - x) == 1 && Mathf.Abs(lastRevealedTile.Value.y - y) == 1;
        private void HandleGridStateChanged() => OnGridUpdated?.Invoke();
    }
}
