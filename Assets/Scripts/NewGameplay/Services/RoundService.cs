// RoundService.cs
using NewGameplay;
using NewGameplay.Interfaces;

public class RoundService : IRoundService
{
    private readonly IGridService grid;
    private IProgressTrackerService progress;
    private readonly IInjectService inject;
    private readonly IDataFragmentService dataFragmentService;
    public event System.Action onRoundReset;
    private bool isResetting = false;

    public RoundService(IGridService grid, IProgressTrackerService progress, IInjectService inject, IDataFragmentService dataFragmentService)
    {
        this.grid = grid;
        this.progress = progress;
        this.inject = inject;
        this.dataFragmentService = dataFragmentService;
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
            
            // Clear the data fragment
            dataFragmentService.ClearFragment();
            
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

    public void CheckDataFragmanentSpawn()
    {
        if (!dataFragmentService.IsFragmentPresent() && progress.HasMetGoal())
        {
            dataFragmentService.SpawnFragment();
        }
    }
}