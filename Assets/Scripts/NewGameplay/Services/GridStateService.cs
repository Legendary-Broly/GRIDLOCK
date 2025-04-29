using System;
using UnityEngine;
using NewGameplay.Interfaces;
using NewGameplay.Models;


namespace NewGameplay.Services
{
    public class GridStateService : IGridStateService
    {
        private readonly int gridWidth = 7;   // Number of columns
        private readonly int gridHeight = 8;  // Number of rows
        private readonly string[,] gridState;
        private readonly bool[,] tilePlayable;
        private TileState[,] tileStates;
        public event Action OnGridStateChanged;

        public int GridSize => gridWidth;  // Keeping GridSize as width for compatibility
        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;
        public string[,] GridState => gridState;
        public bool[,] TilePlayable => tilePlayable;

        public GridStateService()
        {
            gridState = new string[gridWidth, gridHeight];
            tilePlayable = new bool[gridWidth, gridHeight];

            for (int y = 0; y < gridHeight; y++)
                for (int x = 0; x < gridWidth; x++)
                    tilePlayable[x, y] = true;
        }
        public void InitializeTileStates(int width, int height)
        {
            tileStates = new TileState[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    tileStates[x, y] = TileState.Hidden; // default starting state
                }
            }
        }

        public string GetSymbolAt(int x, int y) => gridState[x, y];

        public bool IsTilePlayable(int x, int y) => tilePlayable[x, y];

        public bool IsInBounds(int x, int y) => x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;

        public void SetSymbol(int x, int y, string symbol)
        {
            gridState[x, y] = symbol;
            OnGridStateChanged?.Invoke();
        }

        public void SetTilePlayable(int x, int y, bool playable)
        {
            tilePlayable[x, y] = playable;
            OnGridStateChanged?.Invoke();
        }

        public void ClearAllTiles()
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    gridState[x, y] = null;
                    tilePlayable[x, y] = true;
                }
            }
            OnGridStateChanged?.Invoke();
        }

        public void ClearAllExceptViruses()
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    // Only keep viruses and data fragments
                    if (gridState[x, y] != "X" && gridState[x, y] != "DF")
                    {
                        gridState[x, y] = null;
                        tilePlayable[x, y] = true;
                    }
                }
            }
            OnGridStateChanged?.Invoke();
        }
        public TileState GetTileState(int x, int y)
        {
            if (!IsInBounds(x, y)) return TileState.Hidden;
            return tileStates[x, y];
        }

        public void SetTileState(int x, int y, TileState newState)
        {
            if (!IsInBounds(x, y)) return;
            tileStates[x, y] = newState;
        }
    }
} 