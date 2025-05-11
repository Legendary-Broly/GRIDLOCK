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
            IGridService gridService,
            IProgressTrackerService progressService,
            IDataFragmentService dataFragmentService,
            ICodeShardTracker codeShardTracker,
            ITileElementService tileElementService,
            IRoundService roundService,
            RoundPopupManager roundPopupManager)
        {
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
        }

        private void HandleRoundPopupContinue()
        {
            roundPopupManager.onContinue -= HandleRoundPopupContinue;
            roundService.TriggerRoundReset();
        }
    }
}
