using System.Collections.Generic;

public interface IGridEvaluationStrategy
{
    bool CanEvaluate(int gridSize);
    List<GridStateResult> Evaluate(TileSlotController[,] grid);
}
