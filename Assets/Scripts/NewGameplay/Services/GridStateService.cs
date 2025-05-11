using System;
using UnityEngine;
using NewGameplay.Models;
using NewGameplay.Interfaces;

namespace NewGameplay.Services
{
    public class GridStateService : IGridStateService
    {
        private string[,] gridState;
        private TileState[,] tileStates;
        private bool[,] virusFlags;
        private int gridWidth;
        private int gridHeight;

        public event Action OnGridStateChanged;

        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;


        public void SetGridSize(int width, int height)
        {
            gridWidth = width;
            gridHeight = height;

            gridState = new string[width, height];
            tileStates = new TileState[width, height];
            virusFlags = new bool[width, height];
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    gridState[x, y] = null;
                    tileStates[x, y] = TileState.Hidden;
                    virusFlags[x, y] = false;
                }
            }

            OnGridStateChanged?.Invoke();
        }

        public void ClearAllTiles()
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    gridState[x, y] = null;
                    tileStates[x, y] = TileState.Hidden;
                }
            }

            OnGridStateChanged?.Invoke();
        }

        public void SetTileState(int x, int y, TileState state)
        {
            if (!IsValidTile(x, y)) return;
            tileStates[x, y] = state;
            OnGridStateChanged?.Invoke();
        }

        public TileState GetTileState(int x, int y)
        {
            if (!IsValidTile(x, y)) return TileState.Hidden;
            return tileStates[x, y];
        }

        public void SetGridState(int x, int y, string symbol)
        {
            if (!IsValidTile(x, y)) return;
            gridState[x, y] = symbol;
        }

        public string GetGridState(int x, int y)
        {
            if (!IsValidTile(x, y)) return null;
            return gridState[x, y];
        }

        private bool IsValidTile(int x, int y)
        {
            return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
        }
        public string GetSymbolAt(int x, int y)
        {
            if (!IsValidTile(x, y)) return null;
            return gridState[x, y];
        }
        public void SetVirusFlag(int x, int y, bool flagged)
        {
            if (!IsValidTile(x, y)) return;
            virusFlags[x, y] = flagged;
            OnGridStateChanged?.Invoke();
        }

        public bool IsFlaggedAsVirus(int x, int y)
        {
            return IsValidTile(x, y) && virusFlags[x, y];
        }
    }
}
