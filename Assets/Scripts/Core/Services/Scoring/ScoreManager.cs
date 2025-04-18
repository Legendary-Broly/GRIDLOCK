using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private GridStateEvaluator _evaluator;
    private IGameStateService _state;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _state = GameBootstrapper.GameStateService;
        _evaluator = new GridStateEvaluator(_state);
    }

    public int CalculateTotalScore(TileSlotController[,] grid)
    {
        int baseScore = 0;
        int multiplier = 1;

        foreach (var tile in grid)
        {
            if (tile.HasSymbol())
            {
                int value = tile.GetSymbolValue();
                baseScore += value;
            }
        }

        List<GridStateResult> states = _evaluator.EvaluateStates(grid);

        foreach (var state in states)
        {
            multiplier += state.multiplier;
            Debug.Log($"Matched: {state.name}, x{state.multiplier}");
        }

        float doomMulti = _state.CurrentDoomMultiplier;
        float total = baseScore * multiplier * doomMulti;

        Debug.Log($"[SCORE] Base={baseScore}, Multi={multiplier}, Doom={doomMulti}, Final={total}");

        return Mathf.RoundToInt(total);
    }

    public int RawScore(TileSlotController[,] grid)
    {
        int baseScore = 0;

        foreach (var tile in grid)
        {
            if (tile.HasSymbol())
                baseScore += tile.GetSymbolValue();
        }

        return baseScore;
    }

    public string GridStateSummary(TileSlotController[,] grid)
    {
        var states = _evaluator.EvaluateStates(grid);
        if (states.Count == 0) return "No matches.";

        string summary = "";
        foreach (var state in states)
        {
            summary += $"{state.name} (x{state.multiplier})\n";
        }

        return summary.TrimEnd();
    }

    public List<GridStateResult> EvaluateGrid(TileSlotController[,] grid)
    {
        return _evaluator.EvaluateStates(grid);
    }
}
