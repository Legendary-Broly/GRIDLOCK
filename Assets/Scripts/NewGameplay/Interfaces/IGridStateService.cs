using System;
using UnityEngine;

namespace NewGameplay.Interfaces
{
    public interface IGridStateService
    {
        event Action OnGridStateChanged;
        
        int GridSize { get; }
        string[,] GridState { get; }
        bool[,] TilePlayable { get; }
        
        string GetSymbolAt(int x, int y);
        bool IsTilePlayable(int x, int y);
        bool IsInBounds(int x, int y);
        void SetSymbol(int x, int y, string symbol);
        void SetTilePlayable(int x, int y, bool playable);
        void ClearAllTiles();
        void ClearAllExceptViruses();
    }
} 