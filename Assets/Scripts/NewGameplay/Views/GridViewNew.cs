using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;
using NewGameplay.Views;
using NewGameplay.Enums;
using NewGameplay.Models;

namespace NewGameplay.Views
{
    public class GridViewNew : MonoBehaviour
    {
        [SerializeField] public GameObject gridTilePrefab;
        [SerializeField] public Transform gridParent;
        [SerializeField] public GameObject gridLabelPrefab;
        [SerializeField] public GameObject horizontalDividerPrefab;
        [SerializeField] public GameObject verticalDividerPrefab;
        [SerializeField] public Transform dividerLayer;
        [SerializeField] public GameObject indicatorCornerPrefab;

        private GameObject[,] tiles;
        private TileSlotView[,] slots;
        private TextMeshProUGUI[] columnLabels;
        private TextMeshProUGUI[] rowLabels;

        private int width;
        private int height;

        private HashSet<int> visibleRows = new();
        private HashSet<int> visibleColumns = new();

        public void BuildGrid(
            int width,
            int height,
            Func<int, int, int> getVirusCountInColumn,
            Func<int, int, int> getVirusCountInRow,
            Action<int, int, PointerEventData.InputButton> onTileClicked,
            Action<int, int, TileSlotView> slotInitializer)

        {
            this.width = width;
            this.height = height;

            ClearGrid();

            var layoutGroup = gridParent.GetComponent<GridLayoutGroup>();
            RectTransform panelRect = gridParent.GetComponent<RectTransform>();
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
                        columnLabels[col] = InstantiateLabelCell(getVirusCountInColumn(col, height).ToString());
                        continue;
                    }

                    if (x == 0)
                    {
                        int row = y - 1;
                        rowLabels[row] = InstantiateLabelCell(getVirusCountInRow(row, width).ToString());
                        continue;
                    }

                    int gridX = x - 1;
                    int gridY = y - 1;

                    GameObject slotGO = Instantiate(gridTilePrefab, gridParent);
                    tiles[gridX, gridY] = slotGO;

                    var slot = slotGO.GetComponent<TileSlotView>();
                    slotInitializer(gridX, gridY, slot);
                    slots[gridX, gridY] = slot;

                    var btn = slotGO.GetComponentInChildren<Button>();
                    if (btn != null)
                    {
                        int tx = gridX;
                        int ty = gridY;
                        btn.onClick.AddListener(() => onTileClicked(tx, ty, PointerEventData.InputButton.Left));
                    }
                }
            }

            GenerateDividers(width, height);
        }

        public void RenderTile(int x, int y)
        {
            if (InBounds(x, y))
                slots[x, y]?.Refresh();
        }

        public void RenderGrid()
        {
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    RenderTile(x, y);
        }

        public void SetInteractable(int x, int y, bool interactable)
        {
            if (!InBounds(x, y)) return;

            var button = tiles[x, y]?.transform.Find("Button")?.GetComponent<Button>();
            if (button != null)
                button.interactable = interactable;
        }

        public void RefreshVirusLabels(Func<int, int, int> countColumnFn, Func<int, int, int> countRowFn)
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
            foreach (Transform child in dividerLayer)
                Destroy(child.gameObject);

            for (int x = 0; x < width; x++)
            {
                GameObject vLine = Instantiate(verticalDividerPrefab, dividerLayer);
                RectTransform rt = vLine.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2((x + 1f) / (width + 1), 0f);
                rt.anchorMax = new Vector2((x + 1f) / (width + 1), 1f);
                rt.sizeDelta = new Vector2(2f, 0f);
                rt.anchoredPosition = Vector2.zero;
            }

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
            var go = Instantiate(gridLabelPrefab, gridParent);
            var text = go.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = label;
            return text;
        }

        private void InstantiateIndicatorCorner()
        {
            if (indicatorCornerPrefab != null)
                Instantiate(indicatorCornerPrefab, gridParent);
        }

        private void ClearGrid()
        {
            foreach (Transform child in gridParent)
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
    }
}
