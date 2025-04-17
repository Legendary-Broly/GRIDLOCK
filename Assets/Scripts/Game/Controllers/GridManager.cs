using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [SerializeField] private GameObject gridContainer;
    [SerializeField] private GameObject tileSlotPrefab;

    private List<TileSlotController> gridSlots = new();

    [SerializeField] private TileModifierAssigner modifierAssigner;

    private int currentGridSize = 3;

    private void Start()
    {
        //GenerateGridFromState();
    }

    public void GenerateGridFromState()
    {
        int nextSize = GameBootstrapper.GameStateService.GetCurrentGridSize();
        Debug.Log($"[GRID MANAGER] GenerateGridFromState() reading nextSize = {nextSize}");
        CreateGrid(nextSize);
    }

    public void CreateGrid(int size)
    {
        ClearGrid();
        
        // Update the current grid size to match the new grid
        currentGridSize = size;

        if (gridContainer == null || tileSlotPrefab == null)
            return;

        GridLayoutGroup layout = gridContainer.GetComponent<GridLayoutGroup>();
        if (layout == null)
            return;

        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = size;

        for (int i = 0; i < size * size; i++)
        {
            GameObject tile = Instantiate(tileSlotPrefab, gridContainer.transform);
            if (tile == null) continue;

            TileSlotController controller = tile.GetComponent<TileSlotController>();
            if (controller == null) continue;

            gridSlots.Add(controller);
        }

        modifierAssigner.AssignModifiers(gridSlots);
        Debug.Log($"[GRID MANAGER] Created {size}x{size} grid.");
    }

    private void ClearGrid()
    {
        foreach (Transform child in gridContainer.transform)
        {
            Destroy(child.gameObject);
        }
        gridSlots.Clear();
    }

    public TileSlotController[,] GetTileGrid()
    {
        TileSlotController[,] grid = new TileSlotController[currentGridSize, currentGridSize];

        for (int y = 0; y < currentGridSize; y++)
        {
            for (int x = 0; x < currentGridSize; x++)
            {
                int index = x + y * currentGridSize;
                if (index >= gridSlots.Count) continue;

                grid[x, y] = gridSlots[index];
            }
        }

        return grid;
    }

    public int GetCurrentGridSize()
    {
        return currentGridSize;
    }
}
