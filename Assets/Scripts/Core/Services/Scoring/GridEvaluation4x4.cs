using System.Collections.Generic;

public class GridEvaluation4x4 : IGridEvaluationStrategy
{
    public bool CanEvaluate(int gridSize) => gridSize == 4;

    public List<GridStateResult> Evaluate(TileSlotController[,] grid)
    {
        var results = new List<GridStateResult>();

        if (grid.GetLength(0) != 4 || grid.GetLength(1) != 4)
            return results;

        var diagonals = new List<(int x, int y)>[]
        {
            new() { (0, 0), (1, 1), (2, 2), (3, 3) },
            new() { (3, 0), (2, 1), (1, 2), (0, 3) }
        };

        for (int row = 0; row < 4; row++)
        {
            var line = new List<(int x, int y)>();
            for (int col = 0; col < 4; col++)
                line.Add((col, row));
            if (GridMatchUtils.AllMatch(grid, line, out _))
                results.Add(new GridStateResult("Matching Row", 4));
        }

        for (int col = 0; col < 4; col++)
        {
            var line = new List<(int x, int y)>();
            for (int row = 0; row < 4; row++)
                line.Add((col, row));
            if (GridMatchUtils.AllMatch(grid, line, out _))
                results.Add(new GridStateResult("Matching Column", 4));
        }

        foreach (var diag in diagonals)
        {
            if (GridMatchUtils.AllMatch(grid, diag, out _))
                results.Add(new GridStateResult("Matching Diagonal", 4));
        }

        return results;
    }
}
