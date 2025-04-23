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

    // In RoundService.cs, ResetRound() should only clear grid and symbol bank, not modify the threshold:
    public void ResetRound()
    {
        // Clear the grid, symbol bank, and reset progress
        grid.ClearAllTiles();
        inject.ClearSymbolBank();
        progress.ResetProgress();  // Reset progress for the next round
        onRoundReset?.Invoke(); // Trigger event to update UI
    }

    public void TriggerRoundReset()
    {
        onRoundReset?.Invoke();  // Notify UI listeners
    }

}