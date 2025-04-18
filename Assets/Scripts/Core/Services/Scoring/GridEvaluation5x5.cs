using System.Collections.Generic;

public class GridEvaluation5x5 : IGridEvaluationStrategy
{
    public bool CanEvaluate(int gridSize) => gridSize == 5;

    public List<GridStateResult> Evaluate(TileSlotController[,] grid)
    {
        var results = new List<GridStateResult>();

        if (grid.GetLength(0) != 5 || grid.GetLength(1) != 5)
            return results;

        // x5 Matches: Rows, Columns, Diagonals
        for (int row = 0; row < 5; row++)
        {
            var line = new List<(int x, int y)>();
            for (int col = 0; col < 5; col++) line.Add((col, row));
            if (GridMatchUtils.AllMatch(grid, line, out _))
                results.Add(new GridStateResult($"Row {row}", 5));
        }

        for (int col = 0; col < 5; col++)
        {
            var line = new List<(int x, int y)>();
            for (int row = 0; row < 5; row++) line.Add((col, row));
            if (GridMatchUtils.AllMatch(grid, line, out _))
                results.Add(new GridStateResult($"Column {col}", 5));
        }

        var diag1 = new List<(int x, int y)> { (0, 0), (1, 1), (2, 2), (3, 3), (4, 4) };
        var diag2 = new List<(int x, int y)> { (4, 0), (3, 1), (2, 2), (1, 3), (0, 4) };
        if (GridMatchUtils.AllMatch(grid, diag1, out _)) results.Add(new GridStateResult("Diagonal", 5));
        if (GridMatchUtils.AllMatch(grid, diag2, out _)) results.Add(new GridStateResult("DiagonalL", 5));

        // x9 Patterns
        TryAdd(grid, results, GridMatchUtils.MatchExtraBigX, "Extra Big X", 9);
        TryAdd(grid, results, GridMatchUtils.MatchExtraBigPlus, "Extra Big +", 9);
        TryAdd(grid, results, GridMatchUtils.MatchInsides, "Insides", 9);
        TryAdd(grid, results, GridMatchUtils.MatchTShape, "T Shape", 9);
        TryAdd(grid, results, GridMatchUtils.MatchBigDiamond, "Big Diamond", 9);

        // x13 Patterns
        TryAdd(grid, results, GridMatchUtils.MatchHShape, "H Shape", 13);
        TryAdd(grid, results, GridMatchUtils.MatchMShape, "M Shape", 13);
        TryAdd(grid, results, GridMatchUtils.MatchAShape, "A Shape", 13);
        TryAdd(grid, results, GridMatchUtils.MatchUShape, "U Shape", 13);
        TryAdd(grid, results, GridMatchUtils.MatchWShape, "W Shape", 13);

        // x16 Patterns
        TryAdd(grid, results, GridMatchUtils.MatchOutsides, "Outsides", 16);
        TryAdd(grid, results, GridMatchUtils.MatchNoCorners, "No Corners", 16);

        // x25
        if (GridMatchUtils.MatchFullGrid(grid, out _))
            results.Add(new GridStateResult("Full Matching Grid", 25));

        return results;
    }

    private void TryAdd(TileSlotController[,] grid, List<GridStateResult> results, 
                        System.Func<TileSlotController[,], bool> matchMethod, 
                        string name, int multiplier)
    {
        if (matchMethod(grid))
            results.Add(new GridStateResult(name, multiplier));
    }
}
