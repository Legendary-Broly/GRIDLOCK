using UnityEngine;
using UnityEngine.UI;
using NewGameplay.Interfaces;
using NewGameplay.Services;
using NewGameplay.Models;
using NewGameplay.Enums;

namespace NewGameplay.Controllers
{
    public class ExtractController : MonoBehaviour
    {
        private IProgressTrackerService progressService;
        private IDataFragmentService dataFragmentService;
        private IRoundService roundService;
        private IGridService gridService;
        private ICodeShardTracker codeShardTracker;
        private IExtractService extractService;
        private ITileElementService tileElementService;
        private RoundPopupManager roundPopupManager;

        [SerializeField] private Button extractButton;

        private void Start()
        {
            if (extractButton != null)
            {
                extractButton.interactable = false;
            }
        }

        private void Update()
        {
            if (progressService == null || dataFragmentService == null) return;

            bool canExtract = progressService.HasMetGoal() && !dataFragmentService.AnyRevealedFragmentsContainVirus();
            extractButton.interactable = canExtract;
        }

        public void Initialize(
            IExtractService extractService,
            IGridService gridService,
            IProgressTrackerService progressService,
            IDataFragmentService dataFragmentService,
            ICodeShardTracker codeShardTracker,
            ITileElementService tileElementService,
            IRoundService roundService,
            RoundPopupManager roundPopupManager)
        {
            this.extractService = extractService;
            this.gridService = gridService;
            this.progressService = progressService;
            this.dataFragmentService = dataFragmentService;
            this.codeShardTracker = codeShardTracker;
            this.tileElementService = tileElementService;
            this.roundService = roundService;
            this.roundPopupManager = roundPopupManager;
        }

        public void OnExtractButtonClicked()
        {
            if (progressService.HasMetGoal() && !dataFragmentService.AnyRevealedFragmentsContainVirus())
            {
                Debug.Log("[ExtractController] Extract triggered, showing round popup.");
                roundPopupManager.ShowPopup(roundService.CurrentRound);
                roundPopupManager.onContinue += HandleRoundPopupContinue;
                extractButton.interactable = false;
            }

            for (int y = 0; y < gridService.GridHeight; y++)
            {
                for (int x = 0; x < gridService.GridWidth; x++)
                {
                    var state = gridService.GetTileState(x, y);
                    var element = tileElementService.GetElementAt(x, y);
                    var symbol = gridService.GetSymbolAt(x, y);

                    bool isShard = element == TileElementType.CodeShard;
                    bool isRevealed = state == TileState.Revealed;
                    bool hasVirus = symbol == "X";

                    if (isShard && isRevealed && !hasVirus)
                    {
                        codeShardTracker.AddShard();
                        Debug.Log($"[ExtractController] Code Shard collected at ({x},{y})");
                    }
                }
            }
        }

        private void HandleRoundPopupContinue()
        {
            roundPopupManager.onContinue -= HandleRoundPopupContinue;
            roundService.TriggerRoundReset();
        }
    }
}
