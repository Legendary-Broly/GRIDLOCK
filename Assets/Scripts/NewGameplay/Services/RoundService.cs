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
    private readonly IVirusSpreadService virusSpreadService;

    public event System.Action onRoundReset;
    private bool isResetting = false;

    public RoundService(
        IGridService grid,
        IProgressTrackerService progress,
        IWeightedInjectService inject,
        IDataFragmentService dataFragmentService,
        IVirusSpreadService virusSpreadService)
    {
        this.grid = grid;
        this.progress = progress;
        this.inject = inject;
        this.dataFragmentService = dataFragmentService;
        this.virusSpreadService = virusSpreadService;
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
            if (grid is GridService g) 
            {
                g.SetFirstRevealPermitted(false);
                g.ResetRoundSpawns();
                // Make all tiles playable after clearing
                for (int y = 0; y < g.GridHeight; y++)
                {
                    for (int x = 0; x < g.GridWidth; x++)
                    {
                        g.SetTilePlayable(x, y, true);
                    }
                }
            }

            // Step 3: Reset data fragment progress
            progress.ResetProgress();

            // Step 4: Reset all tiles to hidden state and regenerate tile elements
            if (grid is GridService gridService)
            {
                gridService.InitializeTileStates(gridService.GridWidth, gridService.GridHeight);

                if (gridService.TileElementService != null)
                {
                    gridService.TileElementService.GenerateElements();
                }
            }

            // Step 5: Refresh grid observers (UI, views, etc.)
            grid.TriggerGridUpdate();

            // Step 6: Notify UI or any other listeners
            onRoundReset?.Invoke();

            // Step 7: Lock grid interaction
            grid.LockInteraction();

            // Step 8: Reset inject state
            if (inject is WeightedInjectService weighted)
            {
                weighted.ResetForNewRound();
            }
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