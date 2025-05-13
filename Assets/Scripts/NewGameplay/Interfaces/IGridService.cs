using System;
using System.Collections.Generic;
using UnityEngine;
using NewGameplay.Models;

namespace NewGameplay.Interfaces
{
    public interface IGridService
    {
        event Action OnGridUpdated;
        int GridWidth { get; }
        int GridHeight { get; }
        void SetSymbol(int x, int y, string symbol);
        string GetSymbolAt(int x, int y);
        bool IsInBounds(int x, int y);
        bool IsTileRevealed(int x, int y);
        bool CanRevealTile(int x, int y);
        void SetFirstRevealPermitted(bool value);
        bool IsFirstRevealDone();
        void SetTileState(int x, int y, TileState state);
        TileState GetTileState(int x, int y);
        void RevealTile(int x, int y, bool forceReveal = false);
        void ClearAllTiles();
        void ClearAllExceptViruses();
        void LockInteraction();
        void UnlockInteraction();
        void RefreshTile(int x, int y);
        void TriggerGridUpdate();
        List<Vector2Int> GetAllEmptyTilePositions();
        Vector2Int? GetLastRevealedTile();
        void SetLastRevealedTile(Vector2Int pos);
        void SetVirusFlag(int x, int y, bool flagged);
        bool IsFlaggedAsVirus(int x, int y);
        bool CanUseVirusFlag();
        void DisableVirusFlag();
        void SetSymbolToolService(ISymbolToolService service);
        List<Vector2Int> GetValidInitialRevealPositions();
    }
}