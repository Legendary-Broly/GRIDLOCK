// RoundService.cs
public class RoundService : IRoundService
{
    private readonly IGridService grid;
    private IProgressTrackerService progress;
    private readonly IInjectService inject;
    public event System.Action onRoundReset;
    //private bool roundResetting = false;

    public RoundService(IGridService grid, IProgressTrackerService progress, IInjectService inject)
    {
        this.grid = grid;
        this.progress = progress;
        this.inject = inject;
    }

    public void ResetRound()
    {
        grid.ClearAllTiles();        // Clear grid
        inject.ClearSymbolBank();    // Clear symbol bank
        progress.ResetProgress();    // Reset progress tracker to 0
        onRoundReset?.Invoke();      // Call reset event
    }

    public void TriggerRoundReset()
    {
        onRoundReset?.Invoke();  // Notify UI listeners
    }

}