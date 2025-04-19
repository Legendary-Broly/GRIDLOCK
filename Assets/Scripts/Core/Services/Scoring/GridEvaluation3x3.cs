using System.Collections.Generic;
using UnityEngine;

public class GridEvaluation3x3 : IGridEvaluationStrategy
{
    public bool CanEvaluate(int gridSize)
    {
        return gridSize == 3;
    }

public List<GridStateResult> Evaluate(TileSlotController[,] grid)
{
    var results = new List<GridStateResult>();

    if (grid.GetLength(0) != 3 || grid.GetLength(1) != 3)
        return results;

    // Horizontal matches
    if (GridMatchUtils.AllMatch(grid[0, 0], grid[1, 0], grid[2, 0], out _))
    {
        results.Add(new GridStateResult("Row", 3));
    }
    if (GridMatchUtils.AllMatch(grid[0, 1], grid[1, 1], grid[2, 1], out _))
    {
        results.Add(new GridStateResult("Row", 3));
    }
    if (GridMatchUtils.AllMatch(grid[0, 2], grid[1, 2], grid[2, 2], out _))
    {
        results.Add(new GridStateResult("Row", 3));
    }

    // Vertical matches
    if (GridMatchUtils.AllMatch(grid[0, 0], grid[0, 1], grid[0, 2], out _))
    {
        results.Add(new GridStateResult("Column", 3));
    }
    if (GridMatchUtils.AllMatch(grid[1, 0], grid[1, 1], grid[1, 2], out _))
    {
        results.Add(new GridStateResult("Column", 3));
    }
    if (GridMatchUtils.AllMatch(grid[2, 0], grid[2, 1], grid[2, 2], out _))
    {
        results.Add(new GridStateResult("Column", 3));
    }

    // Diagonal matches
    if (GridMatchUtils.AllMatch(grid[0, 0], grid[1, 1], grid[2, 2], out _))
    {
        results.Add(new GridStateResult("Diagonal", 3));
    }
    if (GridMatchUtils.AllMatch(grid[2, 0], grid[1, 1], grid[0, 2], out _))
    {
        results.Add(new GridStateResult("Diagonal", 3));
    }

    // Additional 3x3 patterns
    if (GridMatchUtils.MatchX(grid, out _))
    {
        results.Add(new GridStateResult("X", 5));
    }

    if (GridMatchUtils.MatchPlus(grid, out _))
    {
        results.Add(new GridStateResult("+", 5));
    }

    if (GridMatchUtils.MatchDiamond(grid, out _))
    {
        results.Add(new GridStateResult("Diamond", 4));
    }

    if (GridMatchUtils.MatchFullGrid(grid, out _))
    {
        results.Add(new GridStateResult("Full Grid", 9));
    }

    return results;
}

}
