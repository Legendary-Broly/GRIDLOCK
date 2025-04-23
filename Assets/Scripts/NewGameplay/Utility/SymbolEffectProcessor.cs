// --- SymbolEffectProcessor.cs ---
using System.Collections.Generic;
using UnityEngine;

public static class SymbolEffectProcessor
{
    public static int Apply(List<Vector2Int> match, IGridService grid, IEntropyService entropy)
    {
        if (match == null || match.Count == 0) return 0;

        string symbol = grid.GetSymbolAt(match[0].x, match[0].y);
        int matchSize = match.Count;
        int score = 0;

        switch (symbol)
        {
            case "Ψ": // Surge
                int basePoints = 1 * matchSize;
                score = basePoints * matchSize; // Apply match multiplier
                entropy.Increase(5);
                Debug.Log($"[Ψ] Surge: {score} pts ({basePoints} base * {matchSize}x multiplier), +5% Entropy");
                break;

            case "∆": // Purge
                score = 1 * matchSize;
                Debug.Log($"[∆] Purge triggered for {matchSize} symbols");
                foreach (var pos in match)
                    PurgeAdjacentViruses(pos.x, pos.y, grid);
                break;

            case "Σ": // Stabilizer
                int entropyReduction = matchSize; // -1 per symbol in match
                entropy.Decrease(entropyReduction);
                Debug.Log($"[Σ] Stabilizer: -{entropyReduction}% Entropy applied for {matchSize} symbols");
                break;

            default:
                Debug.LogWarning($"[SymbolEffect] Unknown symbol '{symbol}' encountered");
                break;
        }
        return score;
    }

    public static void ApplyPassiveEntropyPenalty(IGridService grid, IEntropyService entropy)
    {
        int count = 0;
        for (int y = 0; y < grid.GridSize; y++)
        {
            for (int x = 0; x < grid.GridSize; x++)
            {
                if (grid.GetSymbolAt(x, y) == "X")
                    count++;
            }
        }

        if (count > 0)
        {
            entropy.Increase(count);
            Debug.Log($"[X] Passive virus penalty applied: +{count}% Entropy from {count} viruses");
        }
    }

    public static void ProcessAllLoops(IGridService grid, List<Vector2Int> protectedTiles)
    {
        for (int y = 0; y < grid.GridSize; y++)
        {
            for (int x = 0; x < grid.GridSize; x++)
            {
                if (grid.GetSymbolAt(x, y) == "Θ")
                    DuplicateAdjacentSymbol(x, y, grid, protectedTiles);

            }
        }
    }
    public static void ProcessAllPurges(IGridService grid)
    {
        for (int y = 0; y < grid.GridSize; y++)
        {
            for (int x = 0; x < grid.GridSize; x++)
            {
                if (grid.GetSymbolAt(x, y) == "∆")
                    PurgeAdjacentViruses(x, y, grid);
            }
        }
    }

    private static void PurgeAdjacentViruses(int x, int y, IGridService grid)
    {
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1)
        };

        foreach (var dir in directions)
        {
            int tx = x + dir.x;
            int ty = y + dir.y;

            if (!IsInBounds(tx, ty, grid.GridSize)) continue;
            if (grid.GetSymbolAt(tx, ty) == "X")
            {
                grid.SetSymbol(tx, ty, null);
                Debug.Log($"[∆] Purged virus at ({tx},{ty})");
            }
        }
    }

    private static void DuplicateAdjacentSymbol(int x, int y, IGridService grid, List<Vector2Int> protectedTiles)
    {
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1)
        };

        List<string> nearbySymbols = new();

        foreach (var dir in directions)
        {
            int tx = x + dir.x;
            int ty = y + dir.y;
            if (!IsInBounds(tx, ty, grid.GridSize)) continue;

            string symbol = grid.GetSymbolAt(tx, ty);
            // Exclude only Θ and empty, allow viruses ("X")
            if (!string.IsNullOrEmpty(symbol) && symbol != "Θ")
                nearbySymbols.Add(symbol);
        }

        if (nearbySymbols.Count > 0)
        {
            string duplicate = nearbySymbols[Random.Range(0, nearbySymbols.Count)];
            grid.SetSymbol(x, y, duplicate); // Replace Θ with the duplicated symbol
            protectedTiles.Add(new Vector2Int(x, y)); // Protect duplicated tile
            Debug.Log($"[Θ] Replaced Θ at ({x},{y}) with '{duplicate}'");
        }
        else
        {
            grid.SetSymbol(x, y, null); // Clear if nothing valid to duplicate
            protectedTiles.Add(new Vector2Int(x, y));
            Debug.Log($"[Θ] No symbols to duplicate near ({x},{y}), cleared Θ");
        }
    }

    private static bool IsInBounds(int x, int y, int size)
    {
        return x >= 0 && y >= 0 && x < size && y < size;
    }
    public static int ApplyUnmatchedSymbols(IGridService grid, List<Vector2Int> matchedTiles)
    {
        int score = 0;

        for (int y = 0; y < grid.GridSize; y++)
        {
            for (int x = 0; x < grid.GridSize; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (matchedTiles.Contains(pos)) continue;  // ✅ Skip matched symbols

                string symbol = grid.GetSymbolAt(x, y);
                if (symbol == "Ψ")
                {
                    score += 1;
                    Debug.Log($"[Ψ] Passive unmatched Ψ at ({x},{y}): +1 point");
                }
            }
        }

        return score;
    }

}
