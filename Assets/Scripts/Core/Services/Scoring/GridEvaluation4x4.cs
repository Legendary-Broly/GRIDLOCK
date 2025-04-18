using System.Collections.Generic;

public class GridEvaluation4x4 : IGridEvaluationStrategy
{
    public bool CanEvaluate(int gridSize) => gridSize == 4;

    public List<GridStateResult> Evaluate(TileSlotController[,] grid)
    {
        var results = new List<GridStateResult>();

        if (grid.GetLength(0) != 4 || grid.GetLength(1) != 4)
            return results;

        // x4: Any Row
        for (int row = 0; row < 4; row++)
        {
            var line = new List<(int x, int y)>();
            for (int col = 0; col < 4; col++)
                line.Add((col, row));
            if (GridMatchUtils.AllMatch(grid, line, out _))
                results.Add(new GridStateResult("Any Matching Row", 4));
        }

        // x4: Any Column
        for (int col = 0; col < 4; col++)
        {
            var line = new List<(int x, int y)>();
            for (int row = 0; row < 4; row++)
                line.Add((col, row));
            if (GridMatchUtils.AllMatch(grid, line, out _))
                results.Add(new GridStateResult("Any Matching Column", 4));
        }

        // x4: Any Diagonal
        var diagonals = new List<(int x, int y)>[]
        {
            new() { (0, 0), (1, 1), (2, 2), (3, 3) },
            new() { (3, 0), (2, 1), (1, 2), (0, 3) }
        };
        foreach (var diag in diagonals)
        {
            if (GridMatchUtils.AllMatch(grid, diag, out _))
                results.Add(new GridStateResult("Any Matching Diagonal", 4));
        }

        // x8: No Corners
        var noCorners = new List<(int x, int y)>
        {
            (1, 0), (2, 0),
            (0, 1), (1, 1), (2, 1), (3, 1),
            (1, 2), (2, 2)
        };
        if (GridMatchUtils.AllMatch(grid, noCorners, out _))
            results.Add(new GridStateResult("No Corners", 8));

        // x8: Big X
        var bigX = new List<(int x, int y)>
        {
            (0, 0), (1, 1), (2, 2), (3, 3),
            (3, 0), (2, 1), (1, 2), (0, 3)
        };
        if (GridMatchUtils.AllMatch(grid, bigX, out _))
            results.Add(new GridStateResult("Big X", 8));

        // x8: Checkers
        var checkers = new List<(int x, int y)>
        {
            (0, 0), (2, 0),
            (1, 1), (3, 1),
            (0, 2), (2, 2),
            (1, 3), (3, 3)
        };
        if (GridMatchUtils.AllMatch(grid, checkers, out _))
            results.Add(new GridStateResult("Checkers", 8));

        // x8: Chess
        var chess = new List<(int x, int y)>
        {
            (1, 0), (3, 0),
            (0, 1), (2, 1),
            (1, 2), (3, 2),
            (0, 3), (2, 3)
        };
        if (GridMatchUtils.AllMatch(grid, chess, out _))
            results.Add(new GridStateResult("Chess", 8));

        // x12: Big +
        var bigPlus = new List<(int x, int y)>
        {
            (1, 0), (2, 0),
            (1, 1), (2, 1),
            (0, 2), (1, 2), (2, 2), (3, 2),
            (1, 3), (2, 3)
        };
        if (GridMatchUtils.AllMatch(grid, bigPlus, out _))
            results.Add(new GridStateResult("Big +", 12));

        // x16: Full Grid
        var fullGrid = new List<(int x, int y)>();
        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 4; x++)
                fullGrid.Add((x, y));
        if (GridMatchUtils.AllMatch(grid, fullGrid, out _))
            results.Add(new GridStateResult("Full Matching Grid", 16));

        return results;
    }
}
