// --- SymbolEffectProcessor.cs ---
using System.Collections.Generic;
using NewGameplay.Interfaces;
using UnityEngine;
using NewGameplay.Services;

namespace NewGameplay.Utility
{
    public static class SymbolEffectProcessor
    {
        private static IMutationEffectService mutationEffectService;

        public static void SetMutationEffectService(IMutationEffectService service)
        {
            mutationEffectService = service;
            Debug.Log($"[SymbolEffectProcessor] MutationEffectService set: {(service != null ? "Success" : "Null")}");
        }
        
        public static int Apply(List<Vector2Int> match, IGridService grid, IEntropyService entropy)
        {
            if (match == null || match.Count == 0) return 0;

            string symbol = grid.GetSymbolAt(match[0].x, match[0].y);
            int matchSize = match.Count;
            int score = 0;

            switch (symbol)
            {
                case "Ψ": // Surge
                    int basePoints = 1 * matchSize; // 1 point per symbol
                    score = basePoints * matchSize; // Apply match multiplier
                    Debug.Log($"[Ψ] Surge: {score} pts ({basePoints} base * {matchSize}x multiplier)");
                    break;

                case "∆": // Purge
                    // Temporarily disabled EXTRACT-based purge handling
                    Debug.Log($"[∆] Purge triggered for {matchSize} symbols - EXTRACT handling disabled");
                    break;

                case "Σ": // Stabilizer
                    if (matchSize >= 3) // Only reduce entropy for matches of 3 or more
                    {
                        score = matchSize; // 1 point per symbol
                        int entropyReduction = matchSize * matchSize; // -1 per symbol, multiplied by match size
                        
                        // If "Fear of Change" mutation is active, don't reduce entropy at extraction
                        bool fearOfChangeActive = mutationEffectService != null && 
                                                  mutationEffectService.IsMutationActive(MutationType.FearOfChange);
                        
                        if (!fearOfChangeActive)
                        {
                            entropy.Decrease(entropyReduction);
                            Debug.Log($"[Σ] Stabilizer: {score} pts ({matchSize} symbols), -{entropyReduction}% Entropy (base -{matchSize} * {matchSize} multiplier)");
                        }
                        else
                        {
                            Debug.Log($"[Σ] Stabilizer: {score} pts ({matchSize} symbols), No entropy reduction due to Fear of Change mutation");
                        }
                    }
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
            for (int y = 0; y < grid.GridHeight; y++)
            {
                for (int x = 0; x < grid.GridWidth; x++)
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

        public static void ProcessAllLoops(IGridService grid)
        {
            for (int y = 0; y < grid.GridHeight; y++)
            {
                for (int x = 0; x < grid.GridWidth; x++)
                {
                    if (grid.GetSymbolAt(x, y) == "Θ")
                        DuplicateAdjacentSymbol(x, y, grid);
                }
            }
        }
        public static void ProcessAllPurges(IGridService grid)
        {
            grid.ProcessPurges();
        }

        private static void PurgeAdjacentViruses(int x, int y, IGridService grid)
        {
            // This method is no longer needed as purging is handled by GridService
            Debug.LogWarning("PurgeAdjacentViruses is deprecated. Use GridService.ProcessPurges instead.");
        }

        private static void DuplicateAdjacentSymbol(int x, int y, IGridService grid)
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
                if (!IsInBounds(tx, ty, grid)) continue;

                string symbol = grid.GetSymbolAt(tx, ty);
                // Exclude only Θ and empty, allow viruses ("X")
                if (!string.IsNullOrEmpty(symbol) && symbol != "Θ")
                    nearbySymbols.Add(symbol);
            }

            if (nearbySymbols.Count > 0)
            {
                string duplicate = nearbySymbols[Random.Range(0, nearbySymbols.Count)];
                grid.SetSymbol(x, y, duplicate); // Replace Θ with the duplicated symbol
                Debug.Log($"[Θ] Replaced Θ at ({x},{y}) with '{duplicate}'");
            }
            else
            {
                grid.SetSymbol(x, y, null); // Clear if nothing valid to duplicate
                Debug.Log($"[Θ] No symbols to duplicate near ({x},{y}), cleared Θ");
            }
        }

        private static bool IsInBounds(int x, int y, IGridService grid)
        {
            return x >= 0 && y >= 0 && x < grid.GridWidth && y < grid.GridHeight;
        }
        
        public static int ApplyUnmatchedSymbols(IGridService grid, List<Vector2Int> matchedTiles, IEntropyService entropy)
        {
            int score = 0;

            for (int y = 0; y < grid.GridHeight; y++)
            {
                for (int x = 0; x < grid.GridWidth; x++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (matchedTiles.Contains(pos)) continue;  // Skip matched symbols

                    string symbol = grid.GetSymbolAt(x, y);
                    if (string.IsNullOrEmpty(symbol)) continue;

                    // Never protect purge symbols
                    if (symbol == "∆")
                    {
                        grid.SetSymbol(x, y, null);
                        continue;
                    }

                    // Handle other special symbols
                    switch (symbol)
                    {
                        case "Θ": // Loop
                        case "Σ": // Stabilizer
                        case "Ψ": // Surge - now worth 1 point unmatched
                            score += 1;
                            Debug.Log($"[{symbol}] Unmatched symbol at ({x},{y}): +1 point");
                            break;
                    }
                }
            }

            return score;
        }
    }
}
