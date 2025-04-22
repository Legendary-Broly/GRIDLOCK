using System.Collections.Generic;
using UnityEngine;

public static class GridMatchEvaluator
{
    public static List<List<Vector2Int>> FindMatches(IGridService grid)
    {
        int size = grid.GridSize;
        var visited = new bool[size, size];
        var result = new List<List<Vector2Int>>();

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (visited[x, y]) continue;

                string symbol = grid.GetSymbolAt(x, y);
                if (string.IsNullOrEmpty(symbol) || symbol == "X") continue;

                var cluster = new List<Vector2Int>();
                FloodFill(grid, x, y, symbol, visited, cluster);

                if (cluster.Count >= 3)
                    result.Add(cluster);
            }
        }

        return result;
    }

    private static void FloodFill(IGridService grid, int x, int y, string targetSymbol, bool[,] visited, List<Vector2Int> cluster)
    {
        int size = grid.GridSize;
        if (x < 0 || y < 0 || x >= size || y >= size) return;
        if (visited[x, y]) return;

        string symbol = grid.GetSymbolAt(x, y);
        if (symbol != targetSymbol) return;

        visited[x, y] = true;
        cluster.Add(new Vector2Int(x, y));

        FloodFill(grid, x + 1, y, targetSymbol, visited, cluster);
        FloodFill(grid, x - 1, y, targetSymbol, visited, cluster);
        FloodFill(grid, x, y + 1, targetSymbol, visited, cluster);
        FloodFill(grid, x, y - 1, targetSymbol, visited, cluster);
    }
}
