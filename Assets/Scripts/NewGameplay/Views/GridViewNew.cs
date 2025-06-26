using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;
using NewGameplay.Enums;
using NewGameplay.Models;
using NewGameplay.Interfaces;
using NewGameplay.Controllers;

namespace NewGameplay.Views
{
    public class GridViewNew : MonoBehaviour
    {
        [Header("Grid Prefabs")]
        public GameObject gridTilePrefab;
        public GameObject gridLabelPrefab;
        public GameObject horizontalDividerPrefab;
        public GameObject verticalDividerPrefab;
        public GameObject indicatorCornerPrefab;
        public RectTransform dividerLayer;

        private IGridService gridService;
        private IVirusService virusService;
        private ITileElementService tileElementService;
        private ISymbolToolService symbolToolService;
        private IDataFragmentService dataFragmentService;
        private GridInputController inputController;

        private List<TileSlotView> tileSlots = new List<TileSlotView>();
        private List<GameObject> dividers = new List<GameObject>();
        private List<GameObject> indicators = new List<GameObject>();

        private GameObject[,] tiles;
        private TileSlotView[,] slots;
        private TextMeshProUGUI[] columnLabels;
        private TextMeshProUGUI[] rowLabels;

        private int width;
        private int height;

        private HashSet<int> visibleRows = new();
        private HashSet<int> visibleColumns = new();

        public event Action<int, int, PointerEventData.InputButton> OnTileClicked;

        public void BuildGrid(
            int width,
            int height,
            System.Func<int, int, int> getColumnVirusCount,
            System.Func<int, int, int> getRowVirusCount,
            System.Action<int, int, UnityEngine.EventSystems.PointerEventData.InputButton> onTileClick,
            System.Action<int, int, TileSlotView> onTileCreated)
        {
            this.width = width;
            this.height = height;

            ClearGrid();
            ConfigureGridLayout();
            InitializeGridArrays();
            CreateGridElements(getColumnVirusCount, getRowVirusCount, onTileCreated);
            GenerateDividers(width, height);
        }

        private void ConfigureGridLayout()
        {
            var layoutGroup = GetComponent<GridLayoutGroup>();
            float containerWidth = 1475f;
            float containerHeight = 960f;
            float padding = 5f;
            float panelWidth = containerWidth - padding * 2f;
            float panelHeight = containerHeight - padding * 2f;

            int totalColumns = width + 1;
            int totalRows = height + 1;
            float finalCellWidth = panelWidth / totalColumns;
            float finalCellHeight = panelHeight / totalRows;

            layoutGroup.padding = new RectOffset(0, 0, 0, 0);
            layoutGroup.cellSize = new Vector2(finalCellWidth, finalCellHeight);
            layoutGroup.spacing = Vector2.zero;
            layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layoutGroup.constraintCount = totalColumns;
        }

        private void InitializeGridArrays()
        {
            tiles = new GameObject[width, height];
            slots = new TileSlotView[width, height];
            columnLabels = new TextMeshProUGUI[width];
            rowLabels = new TextMeshProUGUI[height];
        }

        private void CreateGridElements(
            System.Func<int, int, int> getColumnVirusCount,
            System.Func<int, int, int> getRowVirusCount,
            System.Action<int, int, TileSlotView> onTileCreated)
        {
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
                        columnLabels[col] = InstantiateLabelCell(getColumnVirusCount(col, height).ToString());
                        continue;
                    }

                    if (x == 0)
                    {
                        int row = y - 1;
                        rowLabels[row] = InstantiateLabelCell(getRowVirusCount(row, width).ToString());
                        continue;
                    }

                    CreateGridTile(x - 1, y - 1, onTileCreated);
                }
            }
        }

        private void CreateGridTile(int x, int y, System.Action<int, int, TileSlotView> onTileCreated)
        {
            GameObject slotGO = Instantiate(gridTilePrefab, transform);
            tiles[x, y] = slotGO;

            var slot = slotGO.GetComponent<TileSlotView>();
            onTileCreated?.Invoke(x, y, slot);
            slots[x, y] = slot;

            var btn = slotGO.GetComponentInChildren<Button>();
            if (btn != null)
            {
                int tx = x;
                int ty = y;
                btn.onClick.AddListener(() => OnTileClicked?.Invoke(tx, ty, PointerEventData.InputButton.Left));
            }
        }

        public void RenderTile(int x, int y)
        {
            if (InBounds(x, y))
                slots[x, y]?.Refresh();
        }

        public void RenderGrid()
        {
            foreach (var slot in tileSlots)
            {
                slot.UpdateVisuals();
            }
        }

        public void SetInteractable(int x, int y, bool interactable)
        {
            if (!InBounds(x, y)) return;

            var button = tiles[x, y]?.transform.Find("Button")?.GetComponent<Button>();
            if (button != null)
                button.interactable = interactable;
        }

        public void RefreshVirusLabels(System.Func<int, int, int> countColumnFn, System.Func<int, int, int> countRowFn)
        {
            for (int x = 0; x < columnLabels.Length; x++)
                columnLabels[x].text = countColumnFn(x, rowLabels.Length).ToString();

            for (int y = 0; y < rowLabels.Length; y++)
                rowLabels[y].text = countRowFn(y, columnLabels.Length).ToString();
        }

        public void SetVisibleIndicators(int rowCount, int colCount, int maxRows, int maxCols)
        {
            visibleRows = PickUniqueRandomIndices(rowCount, maxRows);
            visibleColumns = PickUniqueRandomIndices(colCount, maxCols);
        }

        public void ApplyIndicatorVisibility()
        {
            for (int x = 0; x < columnLabels.Length; x++)
                columnLabels[x].gameObject.SetActive(visibleColumns.Contains(x));

            for (int y = 0; y < rowLabels.Length; y++)
                rowLabels[y].gameObject.SetActive(visibleRows.Contains(y));
        }

        private void GenerateDividers(int width, int height)
        {
            ClearDividers();
            CreateVerticalDividers(width);
            CreateHorizontalDividers(height);
        }

        private void ClearDividers()
        {
            foreach (Transform child in dividerLayer)
                Destroy(child.gameObject);
        }

        private void CreateVerticalDividers(int width)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject vLine = Instantiate(verticalDividerPrefab, dividerLayer);
                RectTransform rt = vLine.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2((x + 1f) / (width + 1), 0f);
                rt.anchorMax = new Vector2((x + 1f) / (width + 1), 1f);
                rt.sizeDelta = new Vector2(2f, 0f);
                rt.anchoredPosition = Vector2.zero;
            }
        }

        private void CreateHorizontalDividers(int height)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject hLine = Instantiate(horizontalDividerPrefab, dividerLayer);
                RectTransform rt = hLine.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 1f - ((y + 1f) / (height + 1)));
                rt.anchorMax = new Vector2(1f, 1f - ((y + 1f) / (height + 1)));
                rt.sizeDelta = new Vector2(0f, 2f);
                rt.anchoredPosition = Vector2.zero;
            }
        }

        private TextMeshProUGUI InstantiateLabelCell(string label)
        {
            var go = Instantiate(gridLabelPrefab, transform);
            var text = go.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = label;
            return text;
        }

        private void InstantiateIndicatorCorner()
        {
            if (indicatorCornerPrefab != null)
                Instantiate(indicatorCornerPrefab, transform);
        }

        private void ClearGrid()
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);
        }

        private HashSet<int> PickUniqueRandomIndices(int count, int maxExclusive)
        {
            var result = new HashSet<int>();
            while (result.Count < Mathf.Min(count, maxExclusive))
                result.Add(UnityEngine.Random.Range(0, maxExclusive));
            return result;
        }

        private bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < width && y < height;

        private void OnDestroy()
        {
            ClearGrid();
        }
    }
}
