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
    private IGridService gridService;
    private ITileElementService tileElementService;

    public void Initialize(IGridService gridService, ITileElementService tileElementService)
    {
        this.gridService = gridService;
        this.tileElementService = tileElementService;
    }
    // Inside GridViewNew.cs

    public void OnTileClicked(int x, int y)
    {
        if (!gridService.IsTileRevealed(x, y))
        {
            gridService.RevealTile(x, y);
            RefreshGrid(gridService); // Pass gridService instead of this
        }
    }

    public void BuildGrid(int width, int height, System.Action<int, int> onTileClicked)
    {
        Debug.Log($"[GridViewNew] BuildGrid called with width: {width}, height: {height}");
        Debug.Log($"[GridViewNew] gridTilePrefab is {(gridTilePrefab != null ? "assigned" : "NULL")}");
        Debug.Log($"[GridViewNew] gridParent is {(gridParent != null ? "assigned" : "NULL")}");
        
        tiles = new GameObject[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var tile = Instantiate(gridTilePrefab, gridParent);
                int capturedX = x;
                int capturedY = y;
                var button = tile.transform.Find("Button")?.GetComponent<Button>();
                if (button != null)
                    button.onClick.AddListener(() => onTileClicked(capturedX, capturedY));
                else
                    Debug.LogError("Button component not found on grid tile prefab or its children.");
                tiles[x, y] = tile;
            }
        }
    }
    public void RefreshGrid(IGridService grid)
    {
        int width = gridService.GridWidth;
        int height = gridService.GridHeight;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var tile = tiles[x, y];

                var buttonTransform = tile.transform.Find("Button");
                if (buttonTransform == null)
                {
                    Debug.LogError($"[GridViewNew] Missing Button at tile ({x},{y})");
                    continue;
                }

                var symbolText = tile.transform.Find("Button/SymbolText")?.GetComponent<TextMeshProUGUI>();
                var slashText = tile.transform.Find("Button/SlashText")?.GetComponent<TextMeshProUGUI>();
                var backgroundText = tile.transform.Find("Button/ElementText")?.GetComponent<TextMeshProUGUI>();

                if (symbolText == null || slashText == null || backgroundText == null)
                {
                    Debug.LogWarning($"Missing text components on tile ({x},{y})!");
                    continue;
                }

                var tileState = gridService.GetTileState(x, y);
                string symbol = gridService.GetSymbolAt(x, y);
                var elementSO = tileElementService.GetElementSOAt(x, y);

                // Always show SlashText for grid visibility
                slashText.enabled = true;

                if (tileState == TileState.Hidden)
                {
                    // Hidden tiles show grid slashes brightly, hide symbol and element
                    slashText.color = Color.white;
                    symbolText.text = "?";
                    symbolText.color = Color.cyan;
                    backgroundText.text = "";
                }
                else if (tileState == TileState.Revealed)
                {
                    // Revealed tiles can lighten grid slashes
                    slashText.color = new Color(1f, 1f, 1f, 0.5f);

                    if (elementSO != null && elementSO.displayText != "")
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
                        // Special case for Data Fragment
                        slashText.enabled = false;
                        symbolText.text = "𝛟";
                        symbolText.color = Color.yellow;
                    }
                    else if (!string.IsNullOrEmpty(symbol))
                    {
                        // Player-placed symbol
                        slashText.enabled = false;
                        symbolText.text = symbol;
                        symbolText.color = Color.white;
                    }
                    else
                    {
                        // No symbol placed even after reveal
                        symbolText.text = "";
                    }
                }
                else if (tileState == TileState.Flagged)
                {
                    slashText.color = Color.red;
                    symbolText.text = "F";
                    symbolText.color = Color.red;
                    backgroundText.text = "";
                }
            }
        }
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

        if (symbolText == null || slashText == null || backgroundText == null)
        {
            Debug.LogWarning($"Missing text components on tile ({x},{y})!");
            return;
        }

        var tileState = gridService.GetTileState(x, y);
        string symbol = gridService.GetSymbolAt(x, y);
        var elementSO = tileElementService.GetElementSOAt(x, y);

        slashText.enabled = true;

        if (tileState == TileState.Hidden)
        {
            slashText.color = Color.cyan;
            symbolText.text = "?";
            symbolText.color = Color.cyan;
            backgroundText.text = "";
        }
        else if (tileState == TileState.Revealed)
        {
            slashText.color = new Color(1f, 1f, 1f, 0.5f);

            // Only show element if tile has been revealed AND no symbol is placed on it
            if (tileState == TileState.Revealed)
            {
                if (tileState == TileState.Revealed)
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

                    // Optional: fade background text if no symbol is present
                    if (string.IsNullOrEmpty(symbol))
                    {
                        backgroundText.color = new Color(backgroundText.color.r, backgroundText.color.g, backgroundText.color.b, 0.6f); // dim it
                    }
                }
            }
            else
            {
                backgroundText.text = ""; // Keep hidden until revealed
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
                // ✅ Tile has been revealed but no symbol is placed
                symbolText.text = ""; // Or "" if you prefer blank
                symbolText.color = new Color(1f, 1f, 1f, 0.2f); // Very faint gray dot
                slashText.color = new Color(1f, 1f, 1f, 0.2f);  // Dim the slashes too
            }
        }
        else if (tileState == TileState.Flagged)
        {
            slashText.color = Color.red;
            symbolText.text = "F";
            symbolText.color = Color.red;
            backgroundText.text = "";
        }
    }
}
