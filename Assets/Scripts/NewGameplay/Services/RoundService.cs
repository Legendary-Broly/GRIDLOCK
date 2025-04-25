// RoundService.cs
using NewGameplay.Interfaces;

public class RoundService : IRoundService
{
    private readonly IGridService grid;
    private IProgressTrackerService progress;
    private readonly IInjectService inject;
    public event System.Action onRoundReset;
    private bool isResetting = false;

    public RoundService(IGridService grid, IProgressTrackerService progress, IInjectService inject)
    {
        this.grid = grid;
        this.progress = progress;
        this.inject = inject;
    }

    public void ResetRound()
    {
        if (isResetting) return;
        isResetting = true;

        try
        {
            // Clear the symbol bank first and ensure it's cleared
            inject.ClearSymbolBank();
            
            // Then clear the grid
            grid.ClearAllTiles();
            
            // Reset progress for the next round
            progress.ResetProgress();
            
            // Finally, trigger UI updates
            onRoundReset?.Invoke();
        }
        finally
        {
            isResetting = false;
        }
    }

    public void TriggerRoundReset()
    {
        onRoundReset?.Invoke();  // Notify UI listeners
    }
}