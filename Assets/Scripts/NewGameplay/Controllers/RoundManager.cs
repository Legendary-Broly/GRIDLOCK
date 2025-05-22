using NewGameplay.Interfaces;
using UnityEngine;
using NewGameplay.Views;
using NewGameplay.Services;
using NewGameplay.Strategies;
using NewGameplay.ScriptableObjects;
using NewGameplay.Models;
using NewGameplay.Controllers;
using System.Collections.Generic;
using NewGameplay.Enums;
using System;
using System.Linq;


namespace NewGameplay.Controllers
{
    public class RoundManager : MonoBehaviour
    {
        [SerializeField] private GridViewNew gridView;
        [SerializeField] private GridInputController inputController;
        [SerializeField] private InjectController injectController;
        [SerializeField] private SymbolToolService symbolToolService;

        private IRoundService roundService;
        private IGridService gridService;
        private IGridStateService gridStateService;
        private IProgressTrackerService progressService;
        private IDataFragmentService dataFragmentService;
        private IVirusService virusService;
        private ITileElementService tileElementService;
        private RoundPopupManager roundPopupManager;
        private PayloadManager payloadManager;
        private ISystemIntegrityService systemIntegrityService;
        private bool hasStartedAtLeastOneRound = false;
        private readonly HashSet<TileElementType> chosenTileElements = new();
        private int lastIndicatorRewardTier = 3; // Default to 3 for first round


        public void Initialize(
            IRoundService roundService, 
            IGridService gridService,
            IGridStateService gridStateService,
            IProgressTrackerService progressService,
            IDataFragmentService dataFragmentService,
            IVirusService virusService,
            ITileElementService tileElementService,
            SymbolToolService symbolToolService,
            PayloadManager payloadManager,
            ISystemIntegrityService systemIntegrityService,
            RoundPopupManager roundPopupManager)
        {
            this.roundService = roundService;
            this.gridService = gridService;
            this.gridStateService = gridStateService;
            this.progressService = progressService;
            this.dataFragmentService = dataFragmentService;
            this.virusService = virusService;
            this.tileElementService = tileElementService;
            this.symbolToolService = symbolToolService;
            this.payloadManager = payloadManager;
            this.systemIntegrityService = systemIntegrityService;
            this.roundPopupManager = roundPopupManager;

        }

        public void StartFirstRound()
        {
            Debug.Log("Starting first round");
            lastIndicatorRewardTier = 3; // ✅ default for round 1
            ProceedWithNextRound();
        }

        public void StartNextRound()
        {
            if (!hasStartedAtLeastOneRound)
            {
                hasStartedAtLeastOneRound = true;
                return; // Skip reward popup at game start
            }

            var availableTileElements = Enum.GetValues(typeof(TileElementType))
                .Cast<TileElementType>()
                .Where(t =>
                    t != TileElementType.Empty &&
                    t != TileElementType.CodeShard &&
                    t != TileElementType.SystemIntegrityIncrease &&
                    t != TileElementType.ToolRefresh &&
                    t != TileElementType.Virus &&
                    t != TileElementType.DataFragment &&
                    !chosenTileElements.Contains(t))
                .ToList();

            TileElementType selectedTileElement = TileElementType.Empty;

            if (availableTileElements.Count > 0)
            {
                selectedTileElement = availableTileElements[UnityEngine.Random.Range(0, availableTileElements.Count)];
            }

            roundPopupManager.ShowOptions(
                onIntegritySelected: () =>
                {
                    lastIndicatorRewardTier = 3;
                    systemIntegrityService.Increase(25f);
                    ProceedWithNextRound();
                },
                onTileElementSelected: (element) =>
                {
                    lastIndicatorRewardTier = 2;
                    tileElementService.AddManualElement(element);
                    tileElementService.AddToSpawnPool(element);
                    ProceedWithNextRound();
                },
                onPayloadSelected: (payloadId) =>
                {
                    lastIndicatorRewardTier = 1;
                    payloadManager.ActivatePayload(payloadId);
                    ProceedWithNextRound();
                },
                selectedTileElement: selectedTileElement
            );
        }
        public void SetLastIndicatorRewardTier(int count)
        {
            lastIndicatorRewardTier = count;
        }
        public int GetCurrentIndicatorCount()
        {
            return lastIndicatorRewardTier;
        }

        private void ProceedWithNextRound()
        {
            roundService.ResetRound(); // ✅ triggers roundReset event
            gridStateService.RestoreEchoTiles(); // ✅ REAPPLY echo-retained tiles

            int rowCount = lastIndicatorRewardTier;
            int colCount = lastIndicatorRewardTier;

            gridView.SetVisibleIndicators(rowCount, colCount, gridService.GridHeight, gridService.GridWidth);

            RebuildGrid(); // ✅ ensures grid is built before refresh
            gridView.RenderGrid();
            gridService.UnlockInteraction();
            if (injectController != null)
                injectController.SetInjectButtonInteractable(true);
        }


        private void RebuildGrid()
        {
            int width = gridService.GridWidth;
            int height = gridService.GridHeight;

            if (gridView != null)
            {
                gridView.BuildGrid(
                    gridService.GridWidth,
                    gridService.GridHeight,
                    (col, h) => virusService.CountVirusesInColumn(col, h),
                    (row, w) => virusService.CountVirusesInRow(row, w),
                    (x, y, button) => inputController.HandleTileClick(x, y, button),
                    (x, y, slot) =>
                    {
                        slot.Initialize(x, y, gridService, virusService, tileElementService, symbolToolService, (tx, ty, btn) => inputController.HandleTileClick(tx, ty, btn));
                        slot.SetDataFragmentService(dataFragmentService);
                    }
                );

                // Find and rebind the GridInputController
                if (inputController != null)
                {
                    inputController.RebindView(gridView);
                }
            }
        }
    }
}