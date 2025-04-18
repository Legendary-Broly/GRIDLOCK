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

        if (_state == null)
            Debug.LogError("[SCORE MANAGER] GameStateService is null!");

        _evaluator = new GridStateEvaluator(_state);
    }


    public int CalculateTotalScore(TileSlotController[,] grid)
    {
        if (grid == null)
        {
            Debug.LogError("[SCORE MANAGER] Grid is null during score calculation.");
            return 0;
        }

        int baseScore = 0;
        foreach (var tile in grid)
        {
            if (tile != null && tile.IsPlayable())
            {
                baseScore += tile.GetSymbolValue();
            }
        }

        List<GridStateResult> states = _evaluator.EvaluateStates(grid);

        int comboMultiplier = 1;
        foreach (var state in states)
        {
            if (state != null)
            {
                comboMultiplier *= state.multiplier;
                Debug.Log($"[SCORE] Matched: {state.name} x{state.multiplier}");
            }
        }

        float doomMulti = _state != null ? _state.CurrentDoomMultiplier : 1f;
        float total = baseScore * comboMultiplier * doomMulti;

        Debug.Log($"[SCORE] Base={baseScore}, ComboMulti={comboMultiplier}, Doom={doomMulti}, Final={total}");

        return Mathf.RoundToInt(total);
    }


    public int RawScore(TileSlotController[,] grid)
    {
        int baseScore = 0;

        foreach (var tile in grid)
        {
            if (tile == null) continue;
            if (tile.IsPlayable())
            {
                baseScore += tile.GetSymbolValue();
            }
        }

        return baseScore;
    }

    public string GridStateSummary(TileSlotController[,] grid)
    {
        var states = _evaluator.EvaluateStates(grid);
        if (states == null || states.Count == 0)
            return "No matches.";

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
