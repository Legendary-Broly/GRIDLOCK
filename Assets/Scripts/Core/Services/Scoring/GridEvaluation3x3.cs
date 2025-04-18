using System.Collections.Generic;

public class GridEvaluation3x3 : IGridEvaluationStrategy
{
    public bool CanEvaluate(int gridSize) => gridSize == 3;

    public List<GridStateResult> Evaluate(TileSlotController[,] grid)
    {
        var results = new List<GridStateResult>();

        if (grid.GetLength(0) != 3 || grid.GetLength(1) != 3)
            return results;

        if (GridMatchUtils.AllMatch(grid[0, 0], grid[1, 0], grid[2, 0], out _))
            results.Add(new GridStateResult("Top Row", 2));
        if (GridMatchUtils.AllMatch(grid[0, 1], grid[1, 1], grid[2, 1], out _))
            results.Add(new GridStateResult("Middle Row", 2));
        if (GridMatchUtils.AllMatch(grid[0, 2], grid[1, 2], grid[2, 2], out _))
            results.Add(new GridStateResult("Bottom Row", 2));

        if (GridMatchUtils.AllMatch(grid[0, 0], grid[0, 1], grid[0, 2], out _))
            results.Add(new GridStateResult("Left Column", 2));
        if (GridMatchUtils.AllMatch(grid[1, 0], grid[1, 1], grid[1, 2], out _))
            results.Add(new GridStateResult("Middle Column", 2));
        if (GridMatchUtils.AllMatch(grid[2, 0], grid[2, 1], grid[2, 2], out _))
            results.Add(new GridStateResult("Right Column", 2));

        if (GridMatchUtils.AllMatch(grid[0, 0], grid[1, 1], grid[2, 2], out _))
            results.Add(new GridStateResult("Diagonal TL-BR", 3));
        if (GridMatchUtils.AllMatch(grid[2, 0], grid[1, 1], grid[0, 2], out _))
            results.Add(new GridStateResult("Diagonal TR-BL", 3));

        return results;
    }
}
