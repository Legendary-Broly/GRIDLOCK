// RoundService.cs
public class RoundService : IRoundService
{
    private int currentThreshold = 20;
    public int CurrentThreshold => currentThreshold;

    private readonly IGridService grid;
    private IProgressTrackerService progress;
    private readonly IInjectService inject;
    public event System.Action onRoundReset;

    public RoundService(IGridService grid, IProgressTrackerService progress, IInjectService inject)
    {
        this.grid = grid;
        this.progress = progress;
        this.inject = inject;
    }

    public void ResetRound()
    {
        currentThreshold += 20;
        grid.ClearAllTiles();
        inject.ClearSymbolBank();
        progress.ResetProgress();
        progress.IncreaseThreshold();
        onRoundReset?.Invoke();
    }
}