using System;
using UnityEngine;
using NewGameplay.Interfaces;

namespace NewGameplay.Services
{
    public class GridStateService : IGridStateService
    {
        private readonly int gridSize = 7;
        private readonly string[,] gridState;
        private readonly bool[,] tilePlayable;

        public event Action OnGridStateChanged;

        public int GridSize => gridSize;
        public string[,] GridState => gridState;
        public bool[,] TilePlayable => tilePlayable;

        public GridStateService()
        {
            gridState = new string[gridSize, gridSize];
            tilePlayable = new bool[gridSize, gridSize];

            for (int y = 0; y < gridSize; y++)
                for (int x = 0; x < gridSize; x++)
                    tilePlayable[x, y] = true;
        }

        public string GetSymbolAt(int x, int y) => gridState[x, y];

        public bool IsTilePlayable(int x, int y) => tilePlayable[x, y];

        public bool IsInBounds(int x, int y) => x >= 0 && x < gridSize && y >= 0 && y < gridSize;

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
            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    gridState[x, y] = null;
                    tilePlayable[x, y] = true;
                }
            }
            OnGridStateChanged?.Invoke();
        }

        public void ClearAllExceptViruses()
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    if (gridState[x, y] != "X")
                    {
                        gridState[x, y] = null;
                        tilePlayable[x, y] = true;
                    }
                }
            }
            OnGridStateChanged?.Invoke();
        }
    }
} 