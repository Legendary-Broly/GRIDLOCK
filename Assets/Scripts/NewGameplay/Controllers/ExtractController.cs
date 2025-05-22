using UnityEngine;
using UnityEngine.UI;
using NewGameplay.Interfaces;
using NewGameplay.Enums;
using NewGameplay.Services;
using NewGameplay.Controllers;
using System;
using System.Linq;
using System.Collections.Generic;

namespace NewGameplay.Controllers
{
    public class ExtractController : MonoBehaviour
    {
        private IProgressTrackerService progressService;
        private IDataFragmentService dataFragmentService;
        private IRoundService roundService;
        private IGridService gridService;
        private ICodeShardTracker codeShardTracker;
        private ITileElementService tileElementService;
        private IExtractService extractService;
        private RoundPopupManager roundPopupManager;
        private PayloadManager payloadManager;
        private ISystemIntegrityService systemIntegrityService;
        private readonly HashSet<TileElementType> chosenTileElements = new();
        private RoundManager roundManager;
        [SerializeField] private Button extractButton;

        private void Start()
        {
            if (extractButton != null)
                extractButton.interactable = false;
        }

        private void Update()
        {
            if (progressService == null || dataFragmentService == null) return;

            bool canExtract = progressService.HasMetGoal() && !dataFragmentService.AnyRevealedFragmentsContainVirus();
            extractButton.interactable = canExtract;
        }

        public void Initialize(
            IGridService gridService,
            IProgressTrackerService progressService,
            IDataFragmentService dataFragmentService,
            ICodeShardTracker codeShardTracker,
            ITileElementService tileElementService,
            IRoundService roundService,
            RoundPopupManager roundPopupManager,
            IExtractService extractService,
            ISystemIntegrityService systemIntegrityService,
            RoundManager roundManager)
        {
            this.gridService = gridService;
            this.progressService = progressService;
            this.dataFragmentService = dataFragmentService;
            this.codeShardTracker = codeShardTracker;
            this.tileElementService = tileElementService;
            this.roundService = roundService;
            this.roundPopupManager = roundPopupManager;
            this.extractService = extractService;
            this.systemIntegrityService = systemIntegrityService;
            this.roundManager = roundManager;
            this.extractService.OnExtractComplete += HandleExtractComplete;
        }

        public void OnExtractButtonClicked()
        {
            if (progressService.HasMetGoal() && !dataFragmentService.AnyRevealedFragmentsContainVirus())
            {

                extractButton.interactable = false;

                extractService.ExtractGrid(); // Grid is cleared
            }
        }

        private void HandleExtractComplete()
        {
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
                    roundManager.SetLastIndicatorRewardTier(3); // 3 indicators
                    systemIntegrityService.Increase(25f);
                    roundManager.ProceedWithNextRound();
                    gridService.UnlockInteraction();
                },
                onTileElementSelected: (element) =>
                {
                    roundManager.SetLastIndicatorRewardTier(2); // 2 indicators
                    tileElementService.AddToSpawnPool(element);
                    tileElementService.AddManualElement(element);
                    chosenTileElements.Add(element);
                    roundManager.ProceedWithNextRound();
                    gridService.UnlockInteraction();
                },
                onPayloadSelected: (payloadId) =>
                {
                    roundManager.SetLastIndicatorRewardTier(1); // 1 indicator
                    payloadManager?.ActivatePayload(payloadId);
                    roundManager.ProceedWithNextRound();
                    gridService.UnlockInteraction();
                },
                selectedTileElement: selectedTileElement
            );
        }

        public void SetPayloadManager(PayloadManager manager) => payloadManager = manager;
    }
}
