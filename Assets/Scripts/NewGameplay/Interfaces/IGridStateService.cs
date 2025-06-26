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

        void SetGridSize(int width, int height);
        string GetGridState(int x, int y);
        void SetGridState(int x, int y, string state);
        TileState GetTileState(int x, int y);
        void SetTileState(int x, int y, TileState state);
        bool IsFlaggedAsVirus(int x, int y);
        void SetVirusFlag(int x, int y, bool flagged);
        void ClearAllTiles();

        // Service Dependencies
        void SetPayloadService(IPayloadService service);

        string GetSymbolAt(int x, int y);
        void PrecomputeEchoTiles();
        void RestoreEchoTiles();
    }
}