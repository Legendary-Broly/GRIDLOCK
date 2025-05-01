using NewGameplay;
using NewGameplay.Interfaces;
using NewGameplay.Services;
using NewGameplay.Controllers;
public class RoundService : IRoundService
{
    private readonly IGridService grid;
    private readonly IProgressTrackerService progress;
    private readonly IWeightedInjectService inject;
    private readonly IDataFragmentService dataFragmentService;

    public event System.Action onRoundReset;
    private bool isResetting = false;
    
    public RoundService(
        IGridService grid, 
        IProgressTrackerService progress, 
        IWeightedInjectService inject, 
        IDataFragmentService dataFragmentService)
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
            // Step 1: Clear all player-injected symbols
            inject.ClearSymbolBank();

            // Step 2: Clear all tiles including state and symbols
            grid.ClearAllTiles();

            // Step 3: Reset data fragment progress
            progress.ResetProgress();

            // Step 4: Reset all tiles to hidden state
            if (grid is GridService gridService)
            {
                gridService.InitializeTileStates(gridService.GridWidth, gridService.GridHeight);
                
                // Step 5: Regenerate tile elements
                if (gridService.TileElementService != null)
                {
                    gridService.TileElementService.GenerateElements();
                }
            }

            // Step 6: Refresh grid observers (UI, views, etc.)
            grid.TriggerGridUpdate();

            // Step 7: Notify UI or any other listeners
            onRoundReset?.Invoke();

            // Step 8: Lock grid interaction
            grid.LockInteraction(); // ← this method should exist in GridService

        }
        finally
        {
            isResetting = false;
        }
    }

    public void TriggerRoundReset()
    {
        onRoundReset?.Invoke();
    }
}
