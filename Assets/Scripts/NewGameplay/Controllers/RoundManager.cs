using UnityEngine;
using NewGameplay.Interfaces;
using NewGameplay.Controllers;
using NewGameplay.Services;
using System;

namespace NewGameplay.Controllers
{
    public class RoundManager
    {
        private IProgressTrackerService progressService;
        private IDataFragmentService dataFragmentService;
        private RoundPopupManager popupManager;
        private GridViewNew gridView;
        private IGridService gridService;
        private IRoundService roundService;

        private int currentRound = 1;
        public event Action OnRoundStarted;

        public RoundManager(
            IProgressTrackerService progress,
            IDataFragmentService dataFragmentService,
            RoundPopupManager popupManager,
            IRoundService roundService,
            IGridService gridService,
            GridViewNew gridView)
        {
            this.progressService = progress;
            this.dataFragmentService = dataFragmentService;
            this.popupManager = popupManager;
            this.roundService = roundService;
            this.gridService = gridService;
            this.gridView = gridView;

            popupManager.onContinue += BeginNewRound;
        }

        public void Initialize(IRoundService roundService, IProgressTrackerService progressService, RoundPopupManager popupManager, IDataFragmentService dataFragmentService)
        {
            this.roundService = roundService;
            this.progressService = progressService;
            this.popupManager = popupManager;
            this.dataFragmentService = dataFragmentService;
        }

        public void StartFirstRound()
        {
            Debug.Log("[RoundManager] Starting first round");
            currentRound = 1;
            progressService.SetRequiredFragments(1); // First round always requires 1 fragment
            dataFragmentService.SpawnFragments(3);
            OnRoundStarted?.Invoke();
        }

        public void CheckRoundEnd()
        {
            bool goalMet = progressService.HasMetGoal();
            bool noInfectedFragments = !dataFragmentService.AnyRevealedFragmentsContainVirus();

            if (goalMet && noInfectedFragments)
            {
                popupManager.ShowPopup(currentRound);
            }
        }

        public void BeginNewRound()
        {
            Debug.Log("[RoundManager] Beginning new round...");

            // Subscribe to the event BEFORE triggering the reset
            roundService.onRoundReset += () =>
            {
                currentRound++;
                Debug.Log($"[RoundManager] Starting round {currentRound}");
                
                // Set required fragments based on round number (1->1, 2->2, 3->3)
                int requiredFragments = Mathf.Clamp(currentRound, 1, 3);
                Debug.Log($"[RoundManager] Setting required fragments for round {currentRound} to {requiredFragments}");
                progressService.SetRequiredFragments(requiredFragments);

                dataFragmentService.SpawnFragments(3);
                gridView.RefreshGrid(gridService);

                // Find and refresh the progress tracker view
                var progressTrackerView = UnityEngine.Object.FindFirstObjectByType<ProgressTrackerView>();
                if (progressTrackerView != null)
                {
                    Debug.Log("[RoundManager] Refreshing progress tracker view");
                    progressTrackerView.Refresh();
                }
            };

            // Now trigger the reset which will call ResetRound() internally
            roundService.ResetRound();
        }
    }
}
