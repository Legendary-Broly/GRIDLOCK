using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using NewGameplay.Interfaces;
using NewGameplay.Enums;
using NewGameplay.Services;
using NewGameplay.Models;
using System.Linq;
using System.Collections;

public class GridViewNew : MonoBehaviour
{
    [SerializeField] private GameObject gridTilePrefab;
    [SerializeField] private Transform gridParent;

    private GameObject[,] tiles;
    private ColorBlock[,] originalColorBlocks;
    private IGridService gridService;
    private ITileElementService tileElementService;
    private IVirusSpreadService virusSpreadService;
    private const float REVEAL_DELAY = 0.1f; // Delay between reveals in seconds

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
                    originalColorBlocks[x, y] = button.colors;
                    button.interactable = false;
                }
                else
                {
                    Debug.LogError("Button component not found on grid tile prefab or its children.");
                }
                tiles[x, y] = tile;
            }
        }

        if (gridService is NewGameplay.Services.GridService concreteGridService)
        {
            concreteGridService.InitializeTileStates(width, height);
        }

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
            return TileElementType.VirusNest;
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
        var virusHintText = tile.transform.Find("Button/VirusHintText")?.GetComponent<TextMeshProUGUI>();
        var button = tile.transform.Find("Button")?.GetComponent<Button>();
        var slotView = tile.GetComponent<TileSlotView>();

        if (symbolText == null || slashText == null || backgroundText == null || button == null || virusHintText == null)
        {
            Debug.LogWarning($"Missing UI components on tile ({x},{y})!");
            return;
        }

        var tileState = gridService.GetTileState(x, y);
        string symbol = gridService.GetSymbolAt(x, y);
        var elementSO = tileElementService.GetElementSOAt(x, y);

        symbolText.text = "";
        backgroundText.text = "";
        backgroundText.color = Color.white;
        symbolText.color = Color.white;
        slashText.enabled = false;
        virusHintText.text = "";
        button.colors = originalColorBlocks[x, y];

        bool isFirst = !gridService.IsFirstRevealDone();
        bool canReveal = tileState == TileState.Hidden && (isFirst || gridService.CanRevealTile(x, y));
        bool isPurgeSelected = InjectServiceLocator.Service?.SelectedSymbol == "∆:/run_PURGE.exe";
        button.interactable = canReveal || (tileState == TileState.Revealed && isPurgeSelected);

        if (tileState == TileState.Hidden)
        {
            symbolText.text = gridService.CanRevealTile(x, y) ? "+" : "";
            backgroundText.text = "";
            slotView?.SetPlayerRevealed(false);
            slashText.enabled = false;
        }
        else if (tileState == TileState.Revealed)
        {
            slotView?.SetPlayerRevealed(true);
            slashText.enabled = true;

            if (elementSO != null)
            {
                backgroundText.text = elementSO.displayText;
                backgroundText.color = elementSO.displayColor;
            }

            if (symbol == "DF")
            {
                symbolText.text = "ǂ";
                symbolText.color = Color.yellow;
            }
            else if (!string.IsNullOrEmpty(symbol))
            {
                symbolText.text = symbol;
                symbolText.color = Color.white;
            }
            else
            {
                symbolText.text = "";
                symbolText.color = new Color(1f, 1f, 1f, 0.2f);
            }

            if (slotView != null)
            {
                int count = 0;
                Vector2Int[] dirs = new[] {
                    Vector2Int.up, Vector2Int.down,
                    Vector2Int.left, Vector2Int.right
                };

                foreach (var dir in dirs)
                {
                    int nx = x + dir.x;
                    int ny = y + dir.y;
                    if (gridService.IsInBounds(nx, ny) && virusSpreadService.HasVirusAt(nx, ny))
                    {
                        count++;
                    }
                }

                virusHintText.text = new string('.', count);
            }
        }
    }

    public void RevealTilesSequentially(List<Vector2Int> tiles, System.Action onComplete = null)
    {
        StartCoroutine(RevealTilesCoroutine(tiles, onComplete));
    }

    private IEnumerator RevealTilesCoroutine(List<Vector2Int> tiles, System.Action onComplete)
    {
        foreach (var tile in tiles)
        {
            gridService.SetTileState(tile.x, tile.y, TileState.Revealed);
            RefreshTileAt(tile.x, tile.y);
            yield return new WaitForSeconds(REVEAL_DELAY);
        }
        onComplete?.Invoke();
    }
}
