using NewGameplay.Interfaces;
using UnityEngine;
using NewGameplay.Views;
using NewGameplay.Services;
using NewGameplay.Strategies;
using NewGameplay.ScriptableObjects;
using NewGameplay.Models;



namespace NewGameplay.Controllers
{
    public class RoundManager : MonoBehaviour
    {
        [SerializeField] private GridViewNew gridView;
        [SerializeField] private GridInputController inputController;
        [SerializeField] private InjectController injectController;

        private IRoundService roundService;
        private IGridService gridService;
        private IGridStateService gridStateService;
        private IProgressTrackerService progressService;
        private IDataFragmentService dataFragmentService;
        private IVirusService virusService;
        private ITileElementService tileElementService;

        public void Initialize(
            IRoundService roundService, 
            IGridService gridService,
            IGridStateService gridStateService,
            IProgressTrackerService progressService,
            IDataFragmentService dataFragmentService,
            IVirusService virusService,
            ITileElementService tileElementService)
        {
            this.roundService = roundService;
            this.gridService = gridService;
            this.gridStateService = gridStateService;
            this.progressService = progressService;
            this.dataFragmentService = dataFragmentService;
            this.virusService = virusService;
            this.tileElementService = tileElementService;

            // Subscribe to round reset event
            this.roundService.onRoundReset += StartNextRound;
        }

        public void StartFirstRound()
        {
            // Optional: Only trigger UI or animations here
            RebuildGrid();
            if (injectController != null)
                injectController.SetInjectButtonInteractable(true);
        }

        public void StartNextRound()
        {
            roundService.ResetRound();
            gridView.Initialize(gridService, virusService, tileElementService, inputController, inputController.HandleTileClick);
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
                else
                {
                    Debug.LogWarning("[RoundManager] Could not find GridInputController in scene");
                }
            }
            else
            {
                Debug.LogWarning("[RoundManager] GridView is not assigned.");
            }
        }
    }
}
