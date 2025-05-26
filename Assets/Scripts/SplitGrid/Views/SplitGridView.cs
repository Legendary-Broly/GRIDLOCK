using UnityEngine;
using SplitGrid.Interfaces;
using NewGameplay.Views;
using NewGameplay.Interfaces;
using NewGameplay.Controllers;
using UnityEngine.UI;

namespace SplitGrid.Views
{
    public class SplitGridView : MonoBehaviour
    {
        [SerializeField] private GridViewNew gridViewA;
        [SerializeField] private GridViewNew gridViewB;
        [SerializeField] private RectTransform gridContainerA;
        [SerializeField] private RectTransform gridContainerB;
        [SerializeField] private GridLayoutGroup layoutGroupA;
        [SerializeField] private GridLayoutGroup layoutGroupB;

        [Header("Grid Prefabs")]
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private GameObject labelPrefab;
        [SerializeField] private GameObject horizontalDividerPrefab;
        [SerializeField] private GameObject verticalDividerPrefab;
        [SerializeField] private GameObject indicatorCornerPrefab;

        [Header("Divider Layers")]
        [SerializeField] private RectTransform dividerLayerA;
        [SerializeField] private RectTransform dividerLayerB;

        private ISplitGridService splitGridService;
        private ITileElementService tileElementServiceA;
        private ITileElementService tileElementServiceB;
        private IVirusService virusServiceA;
        private IVirusService virusServiceB;
        private ISymbolToolService symbolToolServiceA;
        private ISymbolToolService symbolToolServiceB;
        private IDataFragmentService dataFragmentServiceA;
        private IDataFragmentService dataFragmentServiceB;
        private GridInputController inputController;

        public void Initialize(
            ISplitGridService splitGridService,
            ITileElementService tileElementServiceA,
            ITileElementService tileElementServiceB,
            IVirusService virusServiceA,
            IVirusService virusServiceB,
            ISymbolToolService symbolToolServiceA,
            ISymbolToolService symbolToolServiceB,
            IDataFragmentService dataFragmentServiceA,
            IDataFragmentService dataFragmentServiceB,
            GridInputController inputController)
        {
            this.splitGridService = splitGridService;
            this.tileElementServiceA = tileElementServiceA;
            this.tileElementServiceB = tileElementServiceB;
            this.virusServiceA = virusServiceA;
            this.virusServiceB = virusServiceB;
            this.symbolToolServiceA = symbolToolServiceA;
            this.symbolToolServiceB = symbolToolServiceB;
            this.dataFragmentServiceA = dataFragmentServiceA;
            this.dataFragmentServiceB = dataFragmentServiceB;
            this.inputController = inputController;

            splitGridService.OnGridsUpdated += HandleGridsUpdated;

            gridViewA.gridTilePrefab = tilePrefab;
            gridViewA.gridLabelPrefab = labelPrefab;
            gridViewA.horizontalDividerPrefab = horizontalDividerPrefab;
            gridViewA.verticalDividerPrefab = verticalDividerPrefab;
            gridViewA.indicatorCornerPrefab = indicatorCornerPrefab;
            gridViewA.dividerLayer = dividerLayerA;

            gridViewB.gridTilePrefab = tilePrefab;
            gridViewB.gridLabelPrefab = labelPrefab;
            gridViewB.horizontalDividerPrefab = horizontalDividerPrefab;
            gridViewB.verticalDividerPrefab = verticalDividerPrefab;
            gridViewB.indicatorCornerPrefab = indicatorCornerPrefab;
            gridViewB.dividerLayer = dividerLayerB;
        }

        public void SetGridLayouts(int gridAWidth, int gridAHeight, int gridBWidth, int gridBHeight)
        {
            if (gridViewA != null)
            {
                gridViewA.BuildGrid(
                    gridAWidth,
                    gridAHeight,
                    (col, h) => virusServiceA.CountVirusesInColumn(col, h),
                    (row, w) => virusServiceA.CountVirusesInRow(row, w),
                    (x, y, button) => inputController.HandleTileClick(x, y, button),
                    (x, y, slot) =>
                    {
                        slot.Initialize(x, y, splitGridService.GetGrid(GridID.GridA), virusServiceA, tileElementServiceA, symbolToolServiceA, (tx, ty, btn) => inputController.HandleTileClick(tx, ty, btn));
                        slot.SetDataFragmentService(dataFragmentServiceA);
                    }
                );
                ReflowGridLayout(gridAWidth, gridAHeight, gridContainerA, layoutGroupA);
            }

            if (gridViewB != null)
            {
                gridViewB.BuildGrid(
                    gridBWidth,
                    gridBHeight,
                    (col, h) => virusServiceB.CountVirusesInColumn(col, h),
                    (row, w) => virusServiceB.CountVirusesInRow(row, w),
                    (x, y, button) => inputController.HandleTileClick(x, y, button),
                    (x, y, slot) =>
                    {
                        slot.Initialize(x, y, splitGridService.GetGrid(GridID.GridB), virusServiceB, tileElementServiceB, symbolToolServiceB, (tx, ty, btn) => inputController.HandleTileClick(tx, ty, btn));
                        slot.SetDataFragmentService(dataFragmentServiceB);
                    }
                );
                ReflowGridLayout(gridBWidth, gridBHeight, gridContainerB, layoutGroupB);
            }
        }

        private void ReflowGridLayout(int gridWidth, int gridHeight, RectTransform gridContainer, GridLayoutGroup layoutGroup)
        {
            layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layoutGroup.constraintCount = gridWidth;

            float spacing = layoutGroup.spacing.x;
            float availableWidth = gridContainer.rect.width - ((gridWidth - 1) * spacing);
            float availableHeight = gridContainer.rect.height - ((gridHeight - 1) * spacing);

            float cellWidth = availableWidth / gridWidth;
            float cellHeight = availableHeight / gridHeight;

            layoutGroup.cellSize = new Vector2(cellWidth, cellHeight);
        }

        private void HandleGridsUpdated()
        {
            if (gridViewA != null)
                gridViewA.RenderGrid();
            if (gridViewB != null)
                gridViewB.RenderGrid();
        }

        private void OnDestroy()
        {
            if (splitGridService != null)
                splitGridService.OnGridsUpdated -= HandleGridsUpdated;
        }
    }
} 
