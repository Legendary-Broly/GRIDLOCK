using System;
using UnityEngine;
using NewGameplay.Models;
using NewGameplay.Services;
using NewGameplay.Enums;

namespace NewGameplay.Interfaces
{
    public interface IGridStateService
    {
        event Action OnGridStateChanged;

        int GridWidth { get; }
        int GridHeight { get; }

        string GetGridState(int x, int y);
        void SetGridState(int x, int y, string symbol);

        TileState GetTileState(int x, int y);
        void SetTileState(int x, int y, TileState newState);

        void SetGridSize(int width, int height);
        void ClearAllTiles();
        string GetSymbolAt(int x, int y);
        void SetVirusFlag(int x, int y, bool flagged);
        bool IsFlaggedAsVirus(int x, int y);
        // Optional helper
        // bool IsValidTile(int x, int y);
    }
}