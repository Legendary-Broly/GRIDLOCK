using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using NewGameplay.Interfaces;
using NewGameplay.Enums;
using NewGameplay.Services;
using NewGameplay.Models;
using System.Linq;

public class GridViewNew : MonoBehaviour
{
    [SerializeField] private GameObject gridTilePrefab;
    [SerializeField] private Transform gridParent;

    private GameObject[,] tiles;
    private ColorBlock[,] originalColorBlocks;
    private IGridService gridService;
    private ITileElementService tileElementService;
    private IVirusSpreadService virusSpreadService;


    public void Initialize(IGridService gridService, ITileElementService tileElementService, IVirusSpreadService virusSpreadService)
    {
        this.gridService = gridService;
        this.tileElementService = tileElementService;
        this.virusSpreadService = virusSpreadService;
    }

    public void OnTileClicked(int x, int y)
    {
        if (!gridService.IsTileRevealed(x, y))
        {
            gridService.RevealTile(x, y);
        }
        // Always refresh the grid after any click
        RefreshGrid(gridService);
    }

    public void BuildGrid(int width, int height, System.Action<int, int> onTileClicked)
    {
        Debug.Log($"[GridViewNew] BuildGrid called with width: {width}, height: {height}");
        Debug.Log($"[GridViewNew] gridTilePrefab is {(gridTilePrefab != null ? "assigned" : "NULL")}");
        Debug.Log($"[GridViewNew] gridParent is {(gridParent != null ? "assigned" : "NULL")}");

        tiles = new GameObject[width, height];
        originalColorBlocks = new ColorBlock[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var tile = Instantiate(gridTilePrefab, gridParent);
                var slotView = tile.GetComponent<TileSlotView>();
                if (slotView != null)
                {
                    slotView.Initialize(virusSpreadService, tileElementService, gridService);
                }

                int capturedX = x;
                int capturedY = y;

                var button = tile.transform.Find("Button")?.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() => onTileClicked(capturedX, capturedY));
                    // Store the original color block
                    originalColorBlocks[x, y] = button.colors;
                    // Initially set all buttons to non-interactable
                    button.interactable = false;
                }
                else
                {
                    Debug.LogError("Button component not found on grid tile prefab or its children.");
                }
                tiles[x, y] = tile;
            }
        }

        // Initialize tile states before refreshing the grid
        if (gridService is NewGameplay.Services.GridService concreteGridService)
        {
            concreteGridService.InitializeTileStates(width, height);
        }

        // Refresh the grid to set proper interactability states
        RefreshGrid(gridService);
    }

    public void RefreshGrid(IGridService grid)
    {
        int width = gridService.GridWidth;
        int height = gridService.GridHeight;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                RefreshTileAt(x, y);
            }
        }
    }
    private TileElementType GetElementIncludingVirus(int x, int y)
    {
        if (x < 0 || x >= gridService.GridWidth || y < 0 || y >= gridService.GridHeight)
            return TileElementType.Empty;

        if (virusSpreadService.HasVirusAt(x, y))
        {
            Debug.Log($"[GridView] Virus detected at ({x}, {y})");
            return TileElementType.VirusNest; // TEMP: treat virus as VirusNest for now
        }

        return tileElementService.GetElementAt(x, y);
    }

    public void RefreshTileAt(int x, int y)
    {
        var tile = tiles[x, y];
        var buttonTransform = tile.transform.Find("Button");
        if (buttonTransform == null)
        {
            Debug.LogError($"[GridViewNew] Missing Button at tile ({x},{y})");
            return;
        }

        var symbolText = tile.transform.Find("Button/SymbolText")?.GetComponent<TextMeshProUGUI>();
        var slashText = tile.transform.Find("Button/SlashText")?.GetComponent<TextMeshProUGUI>();
        var backgroundText = tile.transform.Find("Button/ElementText")?.GetComponent<TextMeshProUGUI>();
        var button = tile.transform.Find("Button")?.GetComponent<Button>();

        if (symbolText == null || slashText == null || backgroundText == null || button == null)
        {
            Debug.LogWarning($"Missing UI components on tile ({x},{y})!");
            return;
        }

        var tileState = gridService.GetTileState(x, y);
        string symbol = gridService.GetSymbolAt(x, y);
        var elementSO = tileElementService.GetElementSOAt(x, y);

        // Determine if this tile should be interactable
        bool isFirst = !gridService.IsFirstRevealDone();
        bool canReveal = tileState == TileState.Revealed || isFirst || gridService.CanRevealTile(x, y);

        button.interactable = canReveal;

        slashText.enabled = true;

        if (tileState == TileState.Hidden)
        {
            symbolText.text = "{*}";
            backgroundText.text = "";
        }
        else if (tileState == TileState.Revealed)
        {
            if (elementSO != null)
            {
                backgroundText.text = elementSO.displayText;
                backgroundText.color = elementSO.displayColor;
            }
            else
            {
                backgroundText.text = "";
            }

            if (symbol == "DF")
            {
                slashText.enabled = false;
                symbolText.text = "𝛟";
                symbolText.color = Color.yellow;
            }
            else if (!string.IsNullOrEmpty(symbol))
            {
                slashText.enabled = false;
                symbolText.text = symbol;
                symbolText.color = Color.white;
            }
            else
            {
                symbolText.text = "";
                symbolText.color = new Color(1f, 1f, 1f, 0.2f);
            }

            // Set directional indicators if the tile is revealed
            var slotView = tile.GetComponent<TileSlotView>();
            if (slotView != null)
            {
                int w = gridService.GridWidth;
                int h = gridService.GridHeight;

                TileElementType top    = GetElementIncludingVirus(x, y - 1);
                TileElementType bottom = GetElementIncludingVirus(x, y + 1);
                TileElementType left   = GetElementIncludingVirus(x - 1, y);
                TileElementType right  = GetElementIncludingVirus(x + 1, y);


                slotView.SetDirectionalIndicators(x, y);

                // Update neighboring tiles to hide arrows pointing at this tile
                void HideArrowIfValid(int nx, int ny, System.Action<TileSlotView> hideAction)
                {
                    if (nx >= 0 && nx < w && ny >= 0 && ny < h)
                    {
                        if (gridService.GetTileState(nx, ny) == TileState.Revealed)
                        {
                            var neighbor = tiles[nx, ny].GetComponent<TileSlotView>();
                            if (neighbor != null) hideAction(neighbor);
                        }
                    }
                }

                HideArrowIfValid(x, y - 1, v => v.HideArrowBottom()); // tile above → arrow down
                HideArrowIfValid(x, y + 1, v => v.HideArrowTop());    // tile below → arrow up
                HideArrowIfValid(x - 1, y, v => v.HideArrowRight());  // tile left  → arrow right
                HideArrowIfValid(x + 1, y, v => v.HideArrowLeft());   // tile right → arrow left
                // Also remove arrows on *this* tile that point to already revealed neighbors
                if (gridService.GetTileState(x, y - 1) == TileState.Revealed) slotView.HideArrowTop();
                if (gridService.GetTileState(x, y + 1) == TileState.Revealed) slotView.HideArrowBottom();
                if (gridService.GetTileState(x - 1, y) == TileState.Revealed) slotView.HideArrowLeft();
                if (gridService.GetTileState(x + 1, y) == TileState.Revealed) slotView.HideArrowRight();

            }
        }
    }
}
