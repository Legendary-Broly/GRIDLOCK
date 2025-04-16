using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GridStateEvaluator : IGridStateEvaluator
{
    public int EvaluateTotalScore(TileSlotController[,] grid)
    {
        int totalScore = 0;
        int gridSize = grid.GetLength(0);

        HashSet<Vector2Int> matchedPositions = new();
        List<Vector2Int[]> allMatchedStates = new();

        allMatchedStates.AddRange(MatchRows(grid));
        allMatchedStates.AddRange(MatchColumns(grid));
        allMatchedStates.AddRange(MatchDiagonals(grid));
        allMatchedStates.AddRange(MatchDiamond(grid));
        allMatchedStates.AddRange(MatchX(grid));
        allMatchedStates.AddRange(MatchPlus(grid));
        allMatchedStates.AddRange(MatchFullGrid(grid));

        // STEP 1 & 2: Get final value of each symbol (face + modifier)
        Dictionary<Vector2Int, int> modifiedSymbolValues = new();
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                var slot = grid[x, y];
                var card = slot.CurrentCard;
                if (card == null) continue;

                int value = card.Data.baseValue;

                var mod = slot.GetModifier();
                if (mod != null)
                {
                    switch (mod.modifierType)
                    {
                        case TileModifierType.AddValue:
                            value += mod.amount;
                            break;
                        case TileModifierType.MultiplyValue:
                            value *= mod.amount;
                            break;
                    }
                }

                modifiedSymbolValues[new Vector2Int(x, y)] = value;
            }
        }

        // STEP 3: Apply Grid State multipliers
        foreach (var state in allMatchedStates)
        {
            if (state.Length == 0) continue;

            int stateScore = 0;
            int multiplier = GetMultiplierForState(state.Length);

            foreach (var pos in state)
            {
                if (modifiedSymbolValues.TryGetValue(pos, out int val))
                {
                    stateScore += val;
                    matchedPositions.Add(pos);
                }
            }

            totalScore += stateScore * multiplier;
        }

        // Add unmatched tile values (not multiplied)
        foreach (var kvp in modifiedSymbolValues)
        {
            if (!matchedPositions.Contains(kvp.Key))
                totalScore += kvp.Value;
        }

        // STEP 4: Apply Doom Multiplier
        float doomMultiplier = GameBootstrapper.GameStateService.CurrentMultiplier;
        int finalScore = Mathf.RoundToInt(totalScore * doomMultiplier);

        Debug.Log($"[GRIDLOCK] Raw Score: {totalScore}, Final Score with Doom x{doomMultiplier}: {finalScore}");
        return finalScore;
    }


    private bool AllMatch(SymbolDataSO[] symbols)
    {
        if (symbols.Any(s => s == null)) return false;

        // Extract all fruit names (excluding hybrids)
        var fruitTypes = symbols
            .Where(s => s.symbolType == SymbolType.Fruit)
            .Select(s => s.symbolName)
            .Distinct()
            .ToList();

        // Allow max one fruit type (no mix)
        if (fruitTypes.Count > 1)
            return false;

        // All pairs must match with the base match rules
        for (int i = 0; i < symbols.Length; i++)
        {
            for (int j = i + 1; j < symbols.Length; j++)
            {
                if (!Matches(symbols[i], symbols[j]))
                    return false;
            }
        }

        return true;
    }


    private int GetSymbolValue(SymbolDataSO data)
    {
        return data?.symbolType switch
        {
            SymbolType.Fruit => 1,
            SymbolType.Coin => 2,
            SymbolType.Diamond => 3,
            SymbolType.Seven => 5,
            _ => 0
        };
    }

    private int GetMultiplierForState(int count)
    {
        return count switch
        {
            4 => 4,  // Diamond
            5 => 5,  // X or +
            9 => 9,  // Full Grid
            _ => 3   // Row, Column, Diagonal
        };
    }

    private bool Matches(SymbolDataSO a, SymbolDataSO b)
    {
        if (a == null || b == null) return false;
        if (a == b) return true;

        // Exact fruit type check
        if (a.symbolType == SymbolType.Fruit && b.symbolType == SymbolType.Fruit)
            return a.symbolName == b.symbolName;

        // Hybrid to fruit (allowed only one way per match group)
        if (IsHybrid(a.symbolType) && b.symbolType == SymbolType.Fruit)
            return true;

        if (a.symbolType == SymbolType.Fruit && IsHybrid(b.symbolType))
            return true;

        return false;
    }

    private bool IsHybrid(SymbolType type)
    {
        return type == SymbolType.Coin || type == SymbolType.Diamond || type == SymbolType.Seven;
    }

    private List<Vector2Int[]> MatchRows(TileSlotController[,] grid)
    {
        var matches = new List<Vector2Int[]>();
        for (int y = 0; y < 3; y++)
        {
            var symbols = new[]
            {
                grid[0, y].CurrentCard?.Data,
                grid[1, y].CurrentCard?.Data,
                grid[2, y].CurrentCard?.Data
            };

            if (AllMatch(symbols))
            {
                matches.Add(new[]
                {
                    new Vector2Int(0, y),
                    new Vector2Int(1, y),
                    new Vector2Int(2, y)
                });
            }
        }
        return matches;
    }

    private List<Vector2Int[]> MatchColumns(TileSlotController[,] grid)
    {
        var matches = new List<Vector2Int[]>();
        for (int x = 0; x < 3; x++)
        {
            var symbols = new[]
            {
                grid[x, 0].CurrentCard?.Data,
                grid[x, 1].CurrentCard?.Data,
                grid[x, 2].CurrentCard?.Data
            };

            if (AllMatch(symbols))
            {
                matches.Add(new[]
                {
                    new Vector2Int(x, 0),
                    new Vector2Int(x, 1),
                    new Vector2Int(x, 2)
                });
            }
        }
        return matches;
    }


    private List<Vector2Int[]> MatchDiagonals(TileSlotController[,] grid)
    {
        var matches = new List<Vector2Int[]>();

        var symbolsA = new[]
        {
            grid[0, 0].CurrentCard?.Data,
            grid[1, 1].CurrentCard?.Data,
            grid[2, 2].CurrentCard?.Data
        };

        var symbolsB = new[]
        {
            grid[2, 0].CurrentCard?.Data,
            grid[1, 1].CurrentCard?.Data,
            grid[0, 2].CurrentCard?.Data
        };

        if (AllMatch(symbolsA))
        {
            matches.Add(new[]
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 1),
                new Vector2Int(2, 2)
            });
        }

        if (AllMatch(symbolsB))
        {
            matches.Add(new[]
            {
                new Vector2Int(2, 0),
                new Vector2Int(1, 1),
                new Vector2Int(0, 2)
            });
        }

        return matches;
    }


    private List<Vector2Int[]> MatchDiamond(TileSlotController[,] grid)
    {
        var matches = new List<Vector2Int[]>();

        var diamond = new[]
        {
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(2, 1),
            new Vector2Int(1, 2)
        };

        var baseSymbol = grid[1, 0].CurrentCard?.Data;
        if (Matches(baseSymbol, grid[0, 1].CurrentCard?.Data) &&
            Matches(baseSymbol, grid[2, 1].CurrentCard?.Data) &&
            Matches(baseSymbol, grid[1, 2].CurrentCard?.Data))
        {
            matches.Add(diamond);
        }

        return matches;
    }

    private List<Vector2Int[]> MatchX(TileSlotController[,] grid)
    {
        var matches = new List<Vector2Int[]>();

        var positions = new[]
        {
            new Vector2Int(0,0),
            new Vector2Int(2,0),
            new Vector2Int(1,1),
            new Vector2Int(0,2),
            new Vector2Int(2,2)
        };

        var baseSymbol = grid[1, 1].CurrentCard?.Data;

        if (Matches(baseSymbol, grid[0, 0].CurrentCard?.Data) &&
            Matches(baseSymbol, grid[2, 0].CurrentCard?.Data) &&
            Matches(baseSymbol, grid[0, 2].CurrentCard?.Data) &&
            Matches(baseSymbol, grid[2, 2].CurrentCard?.Data))
        {
            matches.Add(positions);
        }

        return matches;
    }

    private List<Vector2Int[]> MatchPlus(TileSlotController[,] grid)
    {
        var matches = new List<Vector2Int[]>();

        var positions = new[]
        {
            new Vector2Int(1,0),
            new Vector2Int(0,1),
            new Vector2Int(1,1),
            new Vector2Int(2,1),
            new Vector2Int(1,2)
        };

        var baseSymbol = grid[1, 1].CurrentCard?.Data;

        if (Matches(baseSymbol, grid[1, 0].CurrentCard?.Data) &&
            Matches(baseSymbol, grid[0, 1].CurrentCard?.Data) &&
            Matches(baseSymbol, grid[2, 1].CurrentCard?.Data) &&
            Matches(baseSymbol, grid[1, 2].CurrentCard?.Data))
        {
            matches.Add(positions);
        }

        return matches;
    }

    private List<Vector2Int[]> MatchFullGrid(TileSlotController[,] grid)
    {
        var matches = new List<Vector2Int[]>();
        var allPositions = new List<Vector2Int>();
        var baseSymbol = grid[0, 0].CurrentCard?.Data;

        if (baseSymbol == null) return matches;

        for (int x = 0; x < 3; x++)
        for (int y = 0; y < 3; y++)
        {
            if (!Matches(baseSymbol, grid[x, y].CurrentCard?.Data))
                return matches;

            allPositions.Add(new Vector2Int(x, y));
        }

        matches.Add(allPositions.ToArray());
        return matches;
    }
}
