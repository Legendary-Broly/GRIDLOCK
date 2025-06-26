using System;
using UnityEngine;
using NewGameplay.Interfaces;
using NewGameplay.Enums;
using NewGameplay.Views;
using NewGameplay.Models;
using NewGameplay.SplitGrid.Interfaces;
using NewGameplay.SplitGrid.Services;
using NewGameplay.SplitGrid.Views;
using NewGameplay.SplitGrid.Data;
using NewGameplay.SplitGrid.Controllers;

namespace NewGameplay.SplitGrid.Interfaces
{
    public enum GridID
    {
        GridA,
        GridB
    }

    public interface ISplitGridService
    {
        event Action OnGridsUpdated;
        
        // Grid Properties
        int GetGridWidth(GridID gridId);
        int GetGridHeight(GridID gridId);
        Vector2Int? GetLastRevealedTile(GridID gridId);
        
        // Grid State
        void SetGridSize(GridID gridId, int width, int height);
        void ClearGrid(GridID gridId);
        void ResetGrids();
        
        // Grid Services
        IGridService GetGridService(GridID gridId);
        void SetTileElementService(GridID gridId, ITileElementService service);
        void SetSymbolToolService(GridID gridId, ISymbolToolService service);
        void SetVirusService(GridID gridId, IVirusService service);
        void SetDataFragmentService(GridID gridId, IDataFragmentService service);
        void InitializeGrids(int width, int height);
        void ClearAllGrids();
        
        // Grid Operations
        void RevealTile(GridID gridId, int x, int y);
        void SetVirusFlag(GridID gridId, int x, int y, bool flagged);
        bool IsFlaggedAsVirus(GridID gridId, int x, int y);
        bool CanRevealTile(GridID gridId, int x, int y);
        bool IsTileRevealed(GridID gridId, int x, int y);
        bool IsInBounds(GridID gridId, int x, int y);
        
        // Grid Interaction
        void LockInteraction(GridID gridId);
        void UnlockInteraction(GridID gridId);
        void EnableVirusFlag(GridID gridId);
        void DisableVirusFlag(GridID gridId);
        bool CanUseVirusFlag(GridID gridId);
        
        // Tile Operations
        void SetSymbol(GridID gridId, int x, int y, string symbol);
        string GetSymbolAt(GridID gridId, int x, int y);
        void SetTileState(GridID gridId, int x, int y, TileState state);
        TileState GetTileState(GridID gridId, int x, int y);
        
        // Grid Updates
        void TriggerGridUpdate(GridID gridId);
        void SetLastRevealedTile(GridID gridId, Vector2Int pos);
        void HandleVirusFlag(GridID gridId, int x, int y, IChatLogService chatLogService);

    }
} 