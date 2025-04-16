using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [SerializeField] private GameObject gridContainer;
    [SerializeField] private GameObject tileSlotPrefab;

    private List<TileSlotController> gridSlots = new();

    [SerializeField] private TileModifierAssigner modifierAssigner; // Ensure this is assigned in inspector.

    private void Start()
    {
        CreateGrid(3);

        // Assign modifiers after grid creation
        modifierAssigner.AssignModifiers(gridSlots);
    }

    public void CreateGrid(int size)
    {
        Debug.Log("GridManager CreateGrid started");
        Debug.Log("GridContainer: " + gridContainer);

        ClearGrid();

        if (gridContainer == null)
        {
            Debug.LogError("GridContainer not assigned!");
            return;
        }

        if (tileSlotPrefab == null)
        {
            Debug.LogError("TileSlotPrefab not assigned in GridManager!", this);
            return;
        }

        GridLayoutGroup layout = gridContainer.GetComponent<GridLayoutGroup>();
        if (layout == null)
        {
            Debug.LogError("GridContainer is missing GridLayoutGroup!");
            return;
        }

        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = size;

        for (int i = 0; i < size * size; i++)
        {
            Debug.Log($"Creating tile {i}");

            GameObject tile = Instantiate(tileSlotPrefab, gridContainer.transform);
            if (tile == null)
            {
                Debug.LogError("Tile instantiation failed!");
                continue;
            }

            Debug.Log("Tile instantiated successfully");

            TileSlotController controller = tile.GetComponent<TileSlotController>();
            if (controller == null)
            {
                Debug.LogError("TileSlotController missing on prefab!", tile);
                continue;
            }

            gridSlots.Add(controller);

            int index = i;
            Button button = tile.GetComponent<Button>();
            if (button == null)
            {
                Debug.LogError("Button missing on tile prefab!", tile);
                continue;
            }

            // TileSlotController handles its own click logic now, no need to assign anything here.

            Debug.Log($"Tile {i} setup complete");
        }

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
        TileSlotController[,] grid = new TileSlotController[3, 3];

        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                int index = x + y * 3;
                if (index >= gridSlots.Count) continue;

                grid[x, y] = gridSlots[index];
            }
        }

        return grid;
    }

}

