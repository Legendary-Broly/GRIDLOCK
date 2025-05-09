using System;
using UnityEngine;
using UnityEngine.UI;
using NewGameplay.Interfaces;
using NewGameplay.Services;
using NewGameplay.Controllers;
using NewGameplay.Utility;
using UnityEngine.EventSystems;
using NewGameplay.Views;
using NewGameplay.Models;

namespace NewGameplay.Views
{
    public class GridViewNew : MonoBehaviour
    {
        [SerializeField] private GameObject gridTilePrefab;
        [SerializeField] private Transform gridParent;

        private GameObject[,] tiles;
        private ColorBlock[,] originalColorBlocks;
        private TileSlotView[,] slots;
        private IGridService gridService;
        private IVirusService virusService;
        private ITileElementService tileElementService;
        private GridInputController inputController;
        private ISymbolToolService symbolToolService;

        public void Initialize(
            IGridService gridService,
            IVirusService virusService,
            ITileElementService tileElementService,
            GridInputController inputController,
            Action<int, int, PointerEventData.InputButton> onTileClicked,
            ISymbolToolService symbolToolService = null)
        {
            this.gridService = gridService;
            this.virusService = virusService;
            this.tileElementService = tileElementService;
            this.inputController = inputController;
            this.symbolToolService = symbolToolService;
            int width = gridService.GridWidth;
            int height = gridService.GridHeight;

            tiles = new GameObject[width, height];
            originalColorBlocks = new ColorBlock[width, height];

            BuildGrid(width, height, onTileClicked);
        }

        public void BuildGrid(int width, int height, Action<int, int, PointerEventData.InputButton> onTileClicked)
        {
            // Clear previous tiles
            foreach (Transform child in gridParent)
            {
                Destroy(child.gameObject);
            }

            // Calculate tile and spacing dynamically
            if (gridParent == null)            
            Debug.LogError("[GridViewNew] gridParent is null!");
            RectTransform panelRect = gridParent.GetComponent<RectTransform>();
            float panelWidth = panelRect.rect.width;
            float panelHeight = panelRect.rect.height;

            float cellWidth = panelWidth / width;
            float cellHeight = panelHeight / height;
            float cellSize = Mathf.Min(cellWidth, cellHeight);

            float spacingRatio = 0.1f; // 10% of cell size for spacing
            float spacing = cellSize * spacingRatio;
            float finalSize = cellSize - spacing;

            var layoutGroup = gridParent.GetComponent<GridLayoutGroup>();
            if (layoutGroup == null)           
            Debug.LogError("[GridViewNew] Missing GridLayoutGroup on gridParent!");
            
            layoutGroup.cellSize = new Vector2(finalSize, finalSize);
            layoutGroup.spacing = new Vector2(spacing, spacing);
            layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layoutGroup.constraintCount = width;

            tiles = new GameObject[width, height];
            slots = new TileSlotView[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    GameObject slotGO = Instantiate(gridTilePrefab, gridParent);
                    tiles[x, y] = slotGO;                              // ← store the GameObject
                    var slot = slotGO.GetComponent<TileSlotView>();
                    slot.Initialize(virusService, tileElementService, gridService, x, y, inputController, symbolToolService);
                    slots[x, y] = slot;

                    int cx = x;
                    int cy = y;

                    var btn = slotGO.GetComponentInChildren<Button>();
                    if (btn == null) Debug.LogError($"[GridViewNew] No Button on tile prefab!");
                    else btn.onClick.AddListener(() => onTileClicked(cx, cy, PointerEventData.InputButton.Left));
                }
            }
        }

        public void RefreshGrid(IGridService gridService)
        {
            Debug.Log($"[GridViewNew] RefreshGrid called with gridService: {gridService.GetHashCode()} at {Time.time}");
            for (int y = 0; y < gridService.GridHeight; y++)
            {
                for (int x = 0; x < gridService.GridWidth; x++)
                {
                    RefreshTileAt(x, y);
                }
            }
        }

        public void RefreshTileAt(int x, int y)
        {
            if (x < 0 || y < 0 || x >= gridService.GridWidth || y >= gridService.GridHeight) return;

            var tile = tiles[x, y];
            if (tile == null) return;
            var slotView = tile.GetComponent<TileSlotView>();
            slotView?.Refresh();
        }

        public void SetInteractable(int x, int y, bool value)
        {
            if (x < 0 || y < 0 || x >= gridService.GridWidth || y >= gridService.GridHeight) return;

            var button = tiles[x, y]?.transform.Find("Button")?.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = value;
            }
        }
    }
}
