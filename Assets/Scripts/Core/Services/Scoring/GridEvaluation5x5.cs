using System.Collections.Generic;

public class GridEvaluation5x5 : IGridEvaluationStrategy
{
    public bool CanEvaluate(int gridSize) => gridSize == 5;

    public List<GridStateResult> Evaluate(TileSlotController[,] grid)
    {
        var results = new List<GridStateResult>();

        if (grid.GetLength(0) != 5 || grid.GetLength(1) != 5)
            return results;

        // Match 5 Rows
        for (int row = 0; row < 5; row++)
        {
            var line = new List<(int x, int y)>();
            for (int col = 0; col < 5; col++)
                line.Add((col, row));
            if (GridMatchUtils.AllMatch(grid, line, out _))
                results.Add(new GridStateResult($"Matching Row {row}", 5));
        }

        // Match 5 Columns
        for (int col = 0; col < 5; col++)
        {
            var line = new List<(int x, int y)>();
            for (int row = 0; row < 5; row++)
                line.Add((col, row));
            if (GridMatchUtils.AllMatch(grid, line, out _))
                results.Add(new GridStateResult($"Matching Column {col}", 5));
        }

        // Diagonal TL -> BR
        var diag1 = new List<(int x, int y)> { (0, 0), (1, 1), (2, 2), (3, 3), (4, 4) };
        if (GridMatchUtils.AllMatch(grid, diag1, out _))
            results.Add(new GridStateResult("Diagonal TL-BR", 5));

        // Diagonal TR -> BL
        var diag2 = new List<(int x, int y)> { (4, 0), (3, 1), (2, 2), (1, 3), (0, 4) };
        if (GridMatchUtils.AllMatch(grid, diag2, out _))
            results.Add(new GridStateResult("Diagonal TR-BL", 5));

        return results;
    }
}
