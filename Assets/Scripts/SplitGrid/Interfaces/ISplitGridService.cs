using System;
using UnityEngine;
using NewGameplay.Interfaces;
using NewGameplay.Enums;
using NewGameplay.Views;
using NewGameplay.Models;



namespace SplitGrid.Interfaces
{
    public enum GridID
    {
        GridA,
        GridB
    }

    public interface ISplitGridService
    {
        event Action OnGridsUpdated;
        
        // Grid Management
        IGridService GetGrid(GridID gridId);
        void InitializeGrids(int width, int height);
        void ClearAllGrids();
        void ClearAllExceptViruses();
        
        // Tile Operations
        void RevealTile(GridID gridId, int x, int y, bool forceReveal = false);
        bool IsTileRevealed(GridID gridId, int x, int y);
        bool CanRevealTile(GridID gridId, int x, int y);
        void SetSymbol(GridID gridId, int x, int y, string symbol);
        string GetSymbolAt(GridID gridId, int x, int y);
        void SetTileState(GridID gridId, int x, int y, TileState state);
        TileState GetTileState(GridID gridId, int x, int y);
        
        // Grid State
        bool IsInBounds(GridID gridId, int x, int y);
        int GetGridWidth(GridID gridId);
        int GetGridHeight(GridID gridId);
        void LockInteraction(GridID gridId);
        void UnlockInteraction(GridID gridId);
        
        // Virus Operations
        void SetVirusFlag(GridID gridId, int x, int y, bool flagged);
        bool IsFlaggedAsVirus(GridID gridId, int x, int y);
        bool CanUseVirusFlag(GridID gridId);
        void DisableVirusFlag(GridID gridId);
        
        // Grid Updates
        void TriggerGridUpdate(GridID gridId);
        Vector2Int? GetLastRevealedTile(GridID gridId);
        void SetLastRevealedTile(GridID gridId, Vector2Int pos);
        
        // Element Operations
        void SetTileElementService(GridID gridId, ITileElementService service);
        void SetSymbolToolService(GridID gridId, ISymbolToolService service);
    }
} 