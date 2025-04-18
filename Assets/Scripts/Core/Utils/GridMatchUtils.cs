using System.Collections.Generic;

public static class GridMatchUtils
{
    public static bool AllMatch(TileSlotController a, TileSlotController b, TileSlotController c, out string matchedSymbol)
    {
        matchedSymbol = null;
        if (a == null || b == null || c == null) return false;

        string baseSymbol = a.GetSymbolName();
        if (string.IsNullOrEmpty(baseSymbol) || baseSymbol == "None") return false;

        if (IsMatchCompatible(baseSymbol, b.GetSymbolName()) && IsMatchCompatible(baseSymbol, c.GetSymbolName()))
        {
            matchedSymbol = baseSymbol;
            return true;
        }

        return false;
    }

    public static bool AllMatch(TileSlotController[,] grid, List<(int x, int y)> positions, out string matchedSymbol)
    {
        matchedSymbol = null;
        if (positions.Count == 0) return false;

        string baseSymbol = grid[positions[0].x, positions[0].y].GetSymbolName();

        if (string.IsNullOrEmpty(baseSymbol) || baseSymbol == "None") return false;

        foreach (var pos in positions)
        {
            string symbol = grid[pos.x, pos.y].GetSymbolName();
            if (!IsMatchCompatible(baseSymbol, symbol))
                return false;
        }

        matchedSymbol = baseSymbol;
        return true;
    }

    public static bool IsMatchCompatible(string a, string b)
    {
        if (a == b) return true;

        bool aHybrid = IsHybrid(a);
        bool bHybrid = IsHybrid(b);

        if (aHybrid && IsFruit(b)) return true;
        if (bHybrid && IsFruit(a)) return true;

        return false;
    }

    private static bool IsFruit(string name)
    {
        return name is "Cherry" or "Grape" or "Banana";
    }

    private static bool IsHybrid(string name)
    {
        return name is "Coin" or "Diamond" or "7";
    }
}
