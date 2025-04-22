using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class ExtractService : IExtractService
{
    private readonly IGridService grid;
    private readonly IEntropyService entropy;
    private readonly IProgressTrackerService progress;
    private List<Vector2Int> protectedTiles = new();
    public event System.Action onGridUpdated;
    public int CurrentScore { get; private set; }

    public ExtractService(IGridService grid, IEntropyService entropy, IProgressTrackerService progress)
    {
        this.grid = grid;
        this.entropy = entropy;
        this.progress = progress;
    }
    public void ExtractGrid()
    {
        var matches = GridMatchEvaluator.FindMatches(grid);
        Debug.Log($"[EXTRACT] Found {matches.Count} match groups");

        int totalScore = 0;
        foreach (var match in matches)
        {
            if (match.Count == 0) continue;

            string symbol = grid.GetSymbolAt(match[0].x, match[0].y);
            Debug.Log($"[EXTRACT] -> Symbol '{symbol}' -> Size {match.Count}");

            foreach (var pos in match)
            {
                Debug.Log($"  - [{pos.x},{pos.y}]");
            }

            totalScore += SymbolEffectProcessor.Apply(match, grid, entropy);
        }

        SymbolEffectProcessor.ApplyPassiveEntropyPenalty(grid, entropy);
        totalScore += SymbolEffectProcessor.ApplyUnmatchedSymbols(grid);

        progress.ApplyScore(totalScore);
        CurrentScore = totalScore;

        SymbolEffectProcessor.ProcessAllPurges(grid);             // Purge viruses
        SymbolEffectProcessor.ProcessAllLoops(grid, protectedTiles); // Duplicate loops BEFORE clearing

        onGridUpdated?.Invoke(); // Refresh grid BEFORE clearing matches

        // Now clear matched tiles AFTER duplication and visual refresh
        foreach (var match in matches)
        {
            foreach (var pos in match)
            {
                grid.SetSymbol(pos.x, pos.y, null);
            }
        }

        grid.ClearAllExceptViruses(protectedTiles);
        onGridUpdated?.Invoke(); // Final refresh
        protectedTiles.Clear();
    }

    public void ClearProtectedTiles()
    {
        protectedTiles.Clear();
    }

}