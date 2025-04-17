using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int RawScore { get; private set; }
    public int FinalScore { get; private set; }
    public string GridStateSummary { get; private set; }

    private GridStateEvaluator gridStateEvaluator;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        gridStateEvaluator = new GridStateEvaluator();
    }

    public void EvaluateGrid(TileSlotController[,] grid)
    {
        RawScore = gridStateEvaluator.CalculateRawScore(grid);
        GridStateSummary = gridStateEvaluator.GetGridStateSummary(grid);

        float multiplier = GameBootstrapper.GameStateService.CurrentDoomMultiplier;
        FinalScore = Mathf.RoundToInt(RawScore * multiplier);

        // Debug.Log($"[SCORE MANAGER] Base: {RawScore}, States: {GridStateSummary}, Multiplier x{multiplier:0.0} → Final: {FinalScore}");
    }

    public void ResetScore()
    {
        RawScore = 0;
        FinalScore = 0;
        GridStateSummary = "None";
    }
}
