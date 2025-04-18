using System.Collections.Generic;


public class GridStateEvaluator
{
    private readonly IGameStateService _state;
    private readonly List<IGridEvaluationStrategy> _strategies;

    public GridStateEvaluator(IGameStateService state)
    {
        _state = state;

        _strategies = new List<IGridEvaluationStrategy>
        {
            new GridEvaluation3x3(),
            new GridEvaluation4x4(),
            new GridEvaluation5x5()
        };
    }

    public List<GridStateResult> EvaluateStates(TileSlotController[,] grid)
    {
        int size = grid.GetLength(0);
        foreach (var strategy in _strategies)
        {
            if (strategy.CanEvaluate(size))
                return strategy.Evaluate(grid);
        }

        return new List<GridStateResult>();
    }
}