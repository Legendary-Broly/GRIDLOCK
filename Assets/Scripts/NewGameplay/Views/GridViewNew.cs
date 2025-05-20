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
using TMPro;
using System.Collections.Generic;

namespace NewGameplay.Views
{
    public class GridViewNew : MonoBehaviour
    {
        [SerializeField] private GameObject gridTilePrefab;
        [SerializeField] private Transform gridParent;
        [SerializeField] private GameObject gridLabelPrefab;
        [SerializeField] private GameObject horizontalDividerPrefab;
        [SerializeField] private GameObject verticalDividerPrefab;
        [SerializeField] private Transform dividerLayer;
        [SerializeField] private GameObject indicatorCornerPrefab;

        private GameObject[,] tiles;
        private ColorBlock[,] originalColorBlocks;
        private TileSlotView[,] slots;
        private IGridService gridService;
        private IVirusService virusService;
        private ITileElementService tileElementService;
        private GridInputController inputController;
        private ISymbolToolService symbolToolService;
        private TextMeshProUGUI[] columnLabels;
        private TextMeshProUGUI[] rowLabels;
        private HashSet<int> visibleRows = new();
        private HashSet<int> visibleColumns = new();



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
            foreach (Transform child in gridParent)
            {
                Destroy(child.gameObject);
            }

            if (gridParent == null)
            {
                Debug.LogError("[GridViewNew] gridParent is null!");
                return;
            }

            var layoutGroup = gridParent.GetComponent<GridLayoutGroup>();
            if (layoutGroup == null)
            {
                Debug.LogError("[GridViewNew] Missing GridLayoutGroup on gridParent!");
                return;
            }

            RectTransform panelRect = gridParent.GetComponent<RectTransform>();

            // FIXED outer dimensions and inner padding
            float containerWidth = 1475f;
            float containerHeight = 960f;
            float padding = 5f;

            float panelWidth = containerWidth - padding * 2f;   // 1465
            float panelHeight = containerHeight - padding * 2f; // 950

            int totalColumns = width + 1;  // grid + column labels
            int totalRows = height + 1;    // grid + row labels

            float finalCellWidth = panelWidth / totalColumns;
            float finalCellHeight = panelHeight / totalRows;

            // Apply layout
            layoutGroup.padding = new RectOffset(0, 0, 0, 0); // Padding is handled by RectTransform
            layoutGroup.cellSize = new Vector2(finalCellWidth, finalCellHeight);
            layoutGroup.spacing = Vector2.zero;
            layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layoutGroup.constraintCount = totalColumns;

            tiles = new GameObject[width, height];
            slots = new TileSlotView[width, height];
            columnLabels = new TextMeshProUGUI[width];
            rowLabels = new TextMeshProUGUI[height];

            for (int y = 0; y <= height; y++)
            {
                for (int x = 0; x <= width; x++)
                {
                    if (x == 0 && y == 0)
                    {
                        InstantiateIndicatorCorner();
                        continue;
                    }
                    if (y == 0)
                    {
                        int col = x - 1;
                        int virusCount = CountVirusesInColumn(col, height);
                        columnLabels[col] = InstantiateLabelCell(virusCount.ToString());


                        continue;
                    }

                    if (x == 0)
                    {
                        int row = y - 1;
                        int virusCount = CountVirusesInRow(row, width);
                        rowLabels[row] = InstantiateLabelCell(virusCount.ToString());


                        continue;
                    }

                    int gridX = x - 1;
                    int gridY = y - 1;
                    GameObject slotGO = Instantiate(gridTilePrefab, gridParent);
                    tiles[gridX, gridY] = slotGO;
                    var slot = slotGO.GetComponent<TileSlotView>();
                    slot.Initialize(virusService, tileElementService, gridService, gridX, gridY, inputController, symbolToolService);
                    slots[gridX, gridY] = slot;

                    var btn = slotGO.GetComponentInChildren<Button>();
                    if (btn == null)
                        Debug.LogError($"[GridViewNew] No Button on tile prefab!");
                    else
                        btn.onClick.AddListener(() => onTileClicked(gridX, gridY, PointerEventData.InputButton.Left));
                }
            }

            GenerateDividers(width, height);
        }
        public void ApplyIndicatorVisibility()
        {
            if (columnLabels != null)
            {
                for (int x = 0; x < columnLabels.Length; x++)
                {
                    if (columnLabels[x] != null)
                        columnLabels[x].gameObject.SetActive(visibleColumns.Contains(x));
                }
            }

            if (rowLabels != null)
            {
                for (int y = 0; y < rowLabels.Length; y++)
                {
                    if (rowLabels[y] != null)
                        rowLabels[y].gameObject.SetActive(visibleRows.Contains(y));
                }
            }
        }

        private void GenerateDividers(int width, int height)
        {
            foreach (Transform child in dividerLayer)
            {
                Destroy(child.gameObject);
            }

            for (int x = 0; x < width; x++)
            {
                GameObject vLine = Instantiate(verticalDividerPrefab, dividerLayer);
                vLine.name = $"VLine_{x}";
                RectTransform rt = vLine.GetComponent<RectTransform>();
                float anchor = (x + 1f) / (width + 1);
                rt.anchorMin = new Vector2((x + 1f) / (width + 1), 0f);
                rt.anchorMax = new Vector2((x + 1f) / (width + 1), 1f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(2f, 0f); // 2-pixel wide
                rt.anchoredPosition = Vector2.zero;
            }

            for (int y = 0; y < height; y++)
            {
                GameObject hLine = Instantiate(horizontalDividerPrefab, dividerLayer);
                hLine.name = $"HLine_{y}";
                RectTransform rt = hLine.GetComponent<RectTransform>();
                float anchor = 1f - ((y + 1f) / (height + 1));
                rt.anchorMin = new Vector2(0f, 1f - ((y + 1f) / (height + 1)));
                rt.anchorMax = new Vector2(1f, 1f - ((y + 1f) / (height + 1)));
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(0f, 2f); // 2-pixel tall
                rt.anchoredPosition = Vector2.zero;
            }
        }

        public void RefreshGrid(IGridService gridService)
        {
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
        public void RebindSymbolToolService(ISymbolToolService service)
        {
            this.symbolToolService = service;
            int width = gridService.GridWidth;
            int height = gridService.GridHeight;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    slots[x, y]?.SetSymbolToolService(service);
                }
            }
        }
        private int CountVirusesInColumn(int x, int height)
        {
            int count = 0;
            for (int y = 0; y < height; y++)
            {
                if (virusService.HasVirusAt(x, y)) count++;
            }
            return count;
        }

        private int CountVirusesInRow(int y, int width)
        {
            int count = 0;
            for (int x = 0; x < width; x++)
            {
                if (virusService.HasVirusAt(x, y)) count++;
            }
            return count;
        }

        private TextMeshProUGUI InstantiateLabelCell(string label)
        {
            var go = Instantiate(gridLabelPrefab, gridParent);
            var text = go.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = label;
            return text;
        }
        public void RefreshVirusLabels()
        {
            for (int x = 0; x < columnLabels.Length; x++)
            {
                int count = CountVirusesInColumn(x, rowLabels.Length);
                columnLabels[x].text = count.ToString();
            }

            for (int y = 0; y < rowLabels.Length; y++)
            {
                int count = CountVirusesInRow(y, columnLabels.Length);
                rowLabels[y].text = count.ToString();
            }
        }
        public void SetVisibleIndicators(int rowCount, int colCount, int maxRows, int maxCols)
        {
            visibleRows = PickUniqueRandomIndices(rowCount, maxRows);
            visibleColumns = PickUniqueRandomIndices(colCount, maxCols);
        }
        private HashSet<int> PickUniqueRandomIndices(int count, int maxExclusive)
        {
            var result = new HashSet<int>();
            while (result.Count < Mathf.Min(count, maxExclusive))
            {
                result.Add(UnityEngine.Random.Range(0, maxExclusive));
            }
            return result;
        }

        private void InstantiateEmptyLabel()
        {
            Instantiate(gridLabelPrefab, gridParent); // leave it blank
        }
        private void InstantiateIndicatorCorner()
        {
            if (indicatorCornerPrefab != null)
            {
                Instantiate(indicatorCornerPrefab, gridParent);
            }
        }
    }
}