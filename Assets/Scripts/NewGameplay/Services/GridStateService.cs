using System;
using UnityEngine;
using NewGameplay.Interfaces;

namespace NewGameplay.Services
{
    public class GridStateService : IGridStateService
    {
        private readonly int gridWidth = 7;   // Number of columns
        private readonly int gridHeight = 8;  // Number of rows
        private readonly string[,] gridState;
        private readonly bool[,] tilePlayable;

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
            // Save data fragment position before clearing
            Vector2Int? dataFragmentPos = null;
            
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    if (gridState[x, y] == "DF")
                    {
                        dataFragmentPos = new Vector2Int(x, y);
                    }
                    
                    // Clear all tiles regardless of symbol
                    gridState[x, y] = null;
                    tilePlayable[x, y] = true;
                }
            }
            
            // Reapply data fragment if it existed (should be handled by DataFragmentService though)
            if (dataFragmentPos.HasValue)
            {
                Debug.Log($"[GridStateService] Preserving data fragment at {dataFragmentPos.Value}");
                int x = dataFragmentPos.Value.x;
                int y = dataFragmentPos.Value.y;
                gridState[x, y] = "DF";
                tilePlayable[x, y] = false;
            }
            
            OnGridStateChanged?.Invoke();
        }

        public void ClearAllExceptViruses()
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    // Only keep viruses, not data fragments
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