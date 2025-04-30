using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using NewGameplay.Models;

namespace NewGameplay.Interfaces
{
    public interface IGridService
    {
        event Action OnGridUpdated;
        void TryPlaceSymbol(int x, int y);
        void TryPlaceSymbol(int x, int y, string symbol);
        void SpreadVirus();
        void SetSymbol(int x, int y, string symbol);
        string GetSymbolAt(int x, int y);
        bool IsTilePlayable(int x, int y);
        bool IsInBounds(int x, int y);
        int GridSize { get; }
        int GridWidth { get; }
        int GridHeight { get; }
        void ClearAllExceptViruses();
        void ClearAllTiles();
        string[,] GridState { get; }
        bool[,] TilePlayable { get; }
        void TriggerGridUpdate();
        List<Vector2Int> GetAllEmptyTilePositions();
        void SetTilePlayable(int x, int y, bool playable);
        void RevealTile(int x, int y, bool forceReveal = false);

        bool IsTileRevealed(int x, int y);
        TileState GetTileState(int x, int y);
        void RefreshTile(int x, int y);
        bool CanRevealTile(int x, int y);
        bool IsFirstRevealDone();
    }
}