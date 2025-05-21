using NewGameplay.Interfaces;
using UnityEngine;
using NewGameplay.Views;
using NewGameplay.Services;
using System.Collections.Generic;
using NewGameplay.Enums;
using System;
using System.Linq;
using NewGameplay.UI;
using NewGameplay.Data;
using NewGameplay.ScriptableObjects;


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
        private readonly RoundConfigSO roundConfig;
        private readonly RoundConfigDatabase roundConfigDatabase;
        private ISplitGridService splitService;

        [SerializeField] private SplitGridView splitGridView;
        [SerializeField] private SplitGridController splitGridController;
        [SerializeField] private GameObject singleGridContainer; 



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

                    if (roundService.CurrentRound >= 4)
                    {
                        splitService.AddTileElement(element); // 🧠 applies to both grids
                    }
                    else
                    {
                        tileElementService.AddManualElement(element);
                        tileElementService.AddToSpawnPool(element);
                    }

                    ProceedWithNextRound();
                },

                onPayloadSelected: (payloadId) =>
                {
                    lastIndicatorRewardTier = 1;

                    if (roundService.CurrentRound >= 4)
                    {
                        splitService.ApplyPayload(payloadId); // 🧠 new shared method
                    }
                    else
                    {
                        payloadManager.ActivatePayload(payloadId);
                    }

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

            // Enable Split Grid after Round 3
            if (roundService.CurrentRound >= 4)
            {
                Debug.Log("[RoundManager] Enabling Split Grid mode");

                if (singleGridContainer != null)
                    singleGridContainer.SetActive(false); // Hide original grid

                splitGridView.ShowSplitGrid();            // Enable split layout
                splitGridController.RevealCenterTiles();  // Reveal center tiles on both grids
                var splitConfig = roundService.RoundConfig; // Add a getter if needed
                splitGridController.ApplyRoundConfig(splitConfig);

                return;
            }

            gridStateService.RestoreEchoTiles(); // REAPPLY echo-retained tiles
            

            Debug.Log($"[RoundManager] ProceedWithNextRound: gridService={gridService.GridWidth}x{gridService.GridHeight}, tileElementService={tileElementService.GridWidth}x{tileElementService.GridHeight}, virusService={virusService.GetType().Name}");
            gridView.Initialize(gridService, virusService, tileElementService, inputController, inputController.HandleTileClick, symbolToolService);

            if (gridService.GridHeight <= 0 || gridService.GridWidth <= 0)
            {
                Debug.LogWarning("[RoundManager] Grid not initialized — skipping indicator setup.");
                return;
            }

            int rowCount = Mathf.Min(lastIndicatorRewardTier, gridService.GridHeight - 1);
            int colCount = Mathf.Min(lastIndicatorRewardTier, gridService.GridWidth - 1);
            gridView.SetVisibleIndicators(rowCount, colCount, gridService.GridHeight, gridService.GridWidth);

            RebuildGrid();
            gridView.RefreshGrid(gridService);
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
                gridView.BuildGrid(width, height, inputController.HandleTileClick);
                
                // Find and rebind the GridInputController
                if (inputController != null)
                {
                    inputController.RebindView(gridView);
                }
            }
        }
    }
}