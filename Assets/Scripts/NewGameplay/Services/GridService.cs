using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridService : IGridService
{
    private readonly int gridSize = 7;
    private readonly string virusSymbol = "X";
    private readonly string shieldSymbol = "∆";

    private readonly string[,] gridState;
    private readonly bool[,] tilePlayable;
    private readonly System.Random rng = new();

    public int GridSize => gridSize;

    public GridService()
    {
        gridState = new string[gridSize, gridSize];
        tilePlayable = new bool[gridSize, gridSize];

        for (int y = 0; y < gridSize; y++)
            for (int x = 0; x < gridSize; x++)
                tilePlayable[x, y] = true;
    }

    public void TryPlaceSymbol(int x, int y)
    {
        if (!tilePlayable[x, y]) return;

        var symbol = InjectServiceLocator.Service.SelectedSymbol;
        if (string.IsNullOrEmpty(symbol)) return;

        SetSymbol(x, y, symbol);
        tilePlayable[x, y] = false;

        InjectServiceLocator.Service.ClearSelectedSymbol(symbol); // ✅ ensures symbol is removed
    }

    public void SetSymbol(int x, int y, string symbol)
    {
        gridState[x, y] = symbol;
        if (symbol == virusSymbol)
            tilePlayable[x, y] = false;
    }

    public string GetSymbolAt(int x, int y) => gridState[x, y];

    public bool IsTilePlayable(int x, int y) => tilePlayable[x, y];

    public void SpreadVirus()
    {
        List<Vector2Int> existingViruses = new();
        List<Vector2Int> emptySpots = new();

        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                if (gridState[x, y] == virusSymbol)
                    existingViruses.Add(new Vector2Int(x, y));
                else if (tilePlayable[x, y])
                    emptySpots.Add(new Vector2Int(x, y));
            }
        }

        if (emptySpots.Count > 0)
        {
            var spawn = emptySpots[rng.Next(emptySpots.Count)];
            SetSymbol(spawn.x, spawn.y, virusSymbol);
        }

        foreach (var pos in existingViruses)
        {
            if (IsAdjacentToSymbol(pos.x, pos.y, shieldSymbol)) continue;

            Vector2Int[] directions = new[] {
                new Vector2Int(1, 0), new Vector2Int(-1, 0),
                new Vector2Int(0, 1), new Vector2Int(0, -1)
            };

            directions = directions.OrderBy(_ => rng.Next()).ToArray();
            foreach (var dir in directions)
            {
                var t = pos + dir;
                if (IsInBounds(t.x, t.y) && tilePlayable[t.x, t.y])
                {
                    SetSymbol(t.x, t.y, virusSymbol);
                    break;
                }
            }
        }
    }

    private bool IsInBounds(int x, int y) => x >= 0 && x < gridSize && y >= 0 && y < gridSize;

    private bool IsAdjacentToSymbol(int x, int y, string target)
    {
        Vector2Int[] directions = new[] {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1)
        };

        foreach (var d in directions)
        {
            int cx = x + d.x, cy = y + d.y;
            if (IsInBounds(cx, cy) && gridState[cx, cy] == target)
                return true;
        }
        return false;
    }
    public void ClearAllExceptViruses(List<Vector2Int> protectedTiles)
    {
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                // Skip if this tile is protected (e.g., Θ duplicated)
                if (protectedTiles.Contains(pos))
                    continue;

                // Clear all except viruses
                if (gridState[x, y] != "X")
                {
                    gridState[x, y] = null;
                    tilePlayable[x, y] = true;
                }
            }
        }
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
    }
}