using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GridMatchUtils
{
    private static readonly HashSet<string> WildSymbols = new() { "Coin", "Diamond", "Seven" };

    private static bool IsValidMatch(List<string> symbols, out string matchedSymbol)
    {
        matchedSymbol = null;
        var nonWilds = symbols.Where(s => !WildSymbols.Contains(s)).Distinct().ToList();
        var wilds = symbols.Where(s => WildSymbols.Contains(s)).Distinct().ToList();

        if (nonWilds.Count == 1 && wilds.All(w => w == wilds[0]))
        {
            matchedSymbol = nonWilds[0];
            return true;
        }

        if (nonWilds.Count == 0 && wilds.Count == 1)
        {
            matchedSymbol = wilds[0];
            return true;
        }

        return false;
    }

    public static bool AllMatch(TileSlotController a, TileSlotController b, TileSlotController c, out string matchedSymbol)
    {
        matchedSymbol = null;

        if (!a.HasSymbol() || !b.HasSymbol() || !c.HasSymbol())
            return false;

        string symbolA = a.GetSymbolName();
        string symbolB = b.GetSymbolName();
        string symbolC = c.GetSymbolName();

        if (string.IsNullOrEmpty(symbolA) || string.IsNullOrEmpty(symbolB) || string.IsNullOrEmpty(symbolC))
            return false;

        return IsValidMatch(new List<string> { symbolA, symbolB, symbolC }, out matchedSymbol);
    }

    public static bool AllMatch(TileSlotController[,] grid, List<(int x, int y)> positions, out string matchedSymbol)
    {
        matchedSymbol = null;
        if (positions.Count == 0) return false;

        List<string> symbols = new();

        foreach (var pos in positions)
        {
            var tile = grid[pos.x, pos.y];
            if (tile == null || !tile.HasSymbol() || tile.IsUnplayable()) return false;
            var symbol = tile.GetSymbolName();
            if (string.IsNullOrEmpty(symbol) || symbol == "None") return false;
            symbols.Add(symbol);
        }

        return IsValidMatch(symbols, out matchedSymbol);
    }

    public static bool MatchX(TileSlotController[,] grid, out string matchedSymbol) => AllMatch(grid, new() {
        (0, 0), (1, 1), (2, 2),
        (0, 2), (2, 0)
    }, out matchedSymbol);

    public static bool MatchPlus(TileSlotController[,] grid, out string matchedSymbol) => AllMatch(grid, new() {
        (1, 0), (0, 1), (1, 1), (2, 1), (1, 2)
    }, out matchedSymbol);

    public static bool MatchDiamond(TileSlotController[,] grid, out string matchedSymbol) => AllMatch(grid, new() {
        (1, 0), (0, 1), (2, 1), (1, 2)
    }, out matchedSymbol);

    public static bool MatchFullGrid(TileSlotController[,] grid, out string matchedSymbol)
    {
        matchedSymbol = null;
        var positions = new List<(int, int)>();
        int size = grid.GetLength(0);
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                positions.Add((x, y));

        return AllMatch(grid, positions, out matchedSymbol);
    }

    public static bool MatchExtraBigX(TileSlotController[,] grid) => AllMatch(grid, new()
    {
        (0, 0), (1, 1), (2, 2), (3, 3), (4, 4),
        (4, 0), (3, 1), (1, 3), (0, 4)
    }, out _);

    public static bool MatchExtraBigPlus(TileSlotController[,] grid) => AllMatch(grid, new()
    {
        (2, 0),
        (2, 1), (0, 2), (1, 2), (2, 2), (3, 2), (4, 2),
        (2, 3),
        (2, 4)
    }, out _);

    public static bool MatchInsides(TileSlotController[,] grid) => AllMatch(grid, new()
    {
        (1, 1), (2, 1), (3, 1),
        (1, 2), (2, 2), (3, 2),
        (1, 3), (2, 3), (3, 3)
    }, out _);

    public static bool MatchTShape(TileSlotController[,] grid) => AllMatch(grid, new()
    {
        (1, 1), (2, 1), (3, 1),
                (2, 2),
                (2, 3)
    }, out _);

    public static bool MatchBigDiamond(TileSlotController[,] grid) => AllMatch(grid, new()
    {
                (2, 0),
        (1, 1),         (3, 1),
        (0, 2),         (4, 2),
        (1, 3),         (3, 3),
                (2, 4)
    }, out _);

    public static bool MatchHShape(TileSlotController[,] grid) => AllMatch(grid, new()
    {
        (0, 0), (0, 1), (0, 2),
                         (2, 1),
        (4, 0), (4, 1), (4, 2)
    }, out _);

    public static bool MatchMShape(TileSlotController[,] grid) => AllMatch(grid, new()
    {
        (0, 4),         (0, 3), (0, 2),
        (1, 3), (2, 2), (3, 3),
        (4, 4),         (4, 3), (4, 2)
    }, out _);

    public static bool MatchAShape(TileSlotController[,] grid) => AllMatch(grid, new()
    {
                (2, 0),
        (1, 1),         (3, 1),
        (0, 2), (1, 2), (2, 2), (3, 2), (4, 2),
        (0, 3),                 (4, 3),
        (0, 4),                 (4, 4)
    }, out _);

    public static bool MatchUShape(TileSlotController[,] grid) => AllMatch(grid, new()
    {
        (0, 0),                 (4, 0),
        (0, 1),                 (4, 1),
        (0, 2),                 (4, 2),
        (0, 3),                 (4, 3),
        (1, 4), (2, 4), (3, 4)
    }, out _);

    public static bool MatchWShape(TileSlotController[,] grid) => AllMatch(grid, new()
    {
        (0, 0),         (0, 1),
        (1, 2),         (2, 3),         (3, 2),
        (4, 0),         (4, 1)
    }, out _);

    public static bool MatchOutsides(TileSlotController[,] grid)
    {
        var positions = new List<(int, int)>();
        int size = grid.GetLength(0);
        for (int x = 0; x < size; x++)
        {
            positions.Add((x, 0)); // top row
            positions.Add((x, size - 1)); // bottom row
        }
        for (int y = 1; y < size - 1; y++)
        {
            positions.Add((0, y)); // left col
            positions.Add((size - 1, y)); // right col
        }
        return AllMatch(grid, positions, out _);
    }

    public static bool MatchNoCorners(TileSlotController[,] grid)
    {
        var positions = new List<(int, int)>();
        int size = grid.GetLength(0);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if ((x == 0 || x == size - 1) && (y == 0 || y == size - 1))
                    continue; // skip corners
                positions.Add((x, y));
            }
        }
        return AllMatch(grid, positions, out _);
    }
}
