using System.Collections.Generic;

/// <summary>
/// Main interface for the grid state evaluation system.
/// Responsible for delegating evaluation to appropriate strategies.
/// </summary>
public interface IGridStateEvaluator
{
    /// <summary>
    /// Evaluates the grid state and calculates total score
    /// </summary>
    /// <param name="grid">The current grid to evaluate</param>
    /// <returns>The calculated score based on grid patterns</returns>
    int EvaluateTotalScore(TileSlotController[,] grid);
    
    /// <summary>
    /// Evaluates the grid and returns detailed state results
    /// </summary>
    /// <param name="grid">The current grid to evaluate</param>
    /// <returns>A list of grid state results</returns>
    List<GridStateResult> EvaluateStates(TileSlotController[,] grid);
    
    /// <summary>
    /// Gets a summary of the grid states for display purposes
    /// </summary>
    /// <param name="grid">The current grid to evaluate</param>
    /// <returns>A string summarizing the grid states</returns>
    string GetGridStateSummary(TileSlotController[,] grid);
}
