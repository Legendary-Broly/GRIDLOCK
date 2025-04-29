using System;
using NewGameplay.Models;
using UnityEngine;

namespace NewGameplay.Interfaces
{
    public interface IGridStateService
    {
        event Action OnGridStateChanged;
        
        int GridSize { get; }  // Kept for backward compatibility
        int GridWidth { get; }  // Number of columns
        int GridHeight { get; } // Number of rows
        string[,] GridState { get; }
        bool[,] TilePlayable { get; }
        string GetSymbolAt(int x, int y);
        bool IsTilePlayable(int x, int y);
        bool IsInBounds(int x, int y);
        void SetSymbol(int x, int y, string symbol);
        void SetTilePlayable(int x, int y, bool playable);
        void ClearAllTiles();
        void ClearAllExceptViruses();
        TileState GetTileState(int x, int y);
        void SetTileState(int x, int y, TileState newState);
        void InitializeTileStates(int width, int height);
    }
} 