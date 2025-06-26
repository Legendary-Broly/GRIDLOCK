using UnityEngine;
using NewGameplay.Enums;
using NewGameplay.Services;
using NewGameplay.ScriptableObjects;
using NewGameplay.Controllers;
using NewGameplay.Data;
using NewGameplay.Interfaces;
using NewGameplay.Views;
using UnityEngine.UI;
using System;
namespace NewGameplay.Controllers
{
    public class SplitGridController : MonoBehaviour
    {
        private GridService gridA;
        private GridService gridB;
        private VirusService virusA;
        private VirusService virusB;
        private TileElementService tileElementServiceA;
        private TileElementService tileElementServiceB;
        private DataFragmentService dataFragmentServiceA;
        private DataFragmentService dataFragmentServiceB;
        private ISplitGridService splitGridService;
        private SplitFlagManager flagManager;
        private GridInputController inputControllerA;
        private GridInputController inputControllerB;
        private GridViewNew gridViewA;
        private GridViewNew gridViewB;
        private SplitGridInputController splitGridInputController;  
        private IProgressTrackerService progressTrackerService;
        private ProgressTrackerView progressTrackerView;
        private InjectController injectController;
        public void Initialize(
            GridService gridServiceA,
            GridService gridServiceB,
            VirusService virusServiceA,
            VirusService virusServiceB,
            ISplitGridService splitService,
            SplitFlagManager flagManager,
            TileElementService tileA,
            TileElementService tileB,
            DataFragmentService fragmentA,
            DataFragmentService fragmentB,
            GridInputController inputControllerA,
            GridInputController inputControllerB,
            GridViewNew gridViewA,
            GridViewNew gridViewB,
            SplitGridInputController splitGridInputController,
            IProgressTrackerService progressTrackerService,
            ProgressTrackerView progressTrackerView,
            InjectController injectController
            )
        {
            this.gridA = gridServiceA;
            this.gridB = gridServiceB;
            this.virusA = virusServiceA;
            this.virusB = virusServiceB;
            this.tileElementServiceA = tileA;
            this.tileElementServiceB = tileB;
            this.dataFragmentServiceA = fragmentA;
            this.dataFragmentServiceB = fragmentB;
            this.splitGridService = splitService;
            this.flagManager = flagManager;
            this.inputControllerA = inputControllerA;
            this.inputControllerB = inputControllerB;
            this.gridViewA = gridViewA;
            this.gridViewB = gridViewB;
            this.splitGridInputController = splitGridInputController;
            this.progressTrackerService = progressTrackerService;
            this.progressTrackerView = progressTrackerView;
            this.injectController = injectController;
        }

        public void RevealCenterTiles()
        {
            var centerA = new Vector2Int(gridA.GridWidth / 2, gridA.GridHeight / 2);
            var centerB = new Vector2Int(gridB.GridWidth / 2, gridB.GridHeight / 2);

            gridA.RevealTile(centerA.x, centerA.y, forceReveal: true);
            gridB.RevealTile(centerB.x, centerB.y, forceReveal: true);
        }

        public void AttemptFlag(GridID gridID, int x, int y)
        {
            if (!flagManager.CanFlag(gridID)) return;

            var targetGrid = gridID == GridID.A ? gridA : gridB;
            var targetVirusService = gridID == GridID.A ? virusA : virusB;

            if (!targetGrid.IsTileRevealed(x, y) && targetGrid.GetLastRevealedTile().HasValue)
            {
                var last = targetGrid.GetLastRevealedTile().Value;
                bool isAdjacent = Mathf.Abs(last.x - x) + Mathf.Abs(last.y - y) == 1;

                if (isAdjacent)
                {
                    bool isVirus = targetVirusService.HasVirusAt(x, y);
                    targetGrid.SetVirusFlag(x, y, isVirus);
                    flagManager.UseFlag(gridID);
                    targetGrid.TriggerGridUpdate();
                }
            }
        }

        public void ApplyRoundConfig(RoundConfigSO config)
        {
            if (!config.useSplitGrid) return;

            if (gridA == null || gridB == null ||
                gridViewA == null || gridViewB == null ||
                inputControllerA == null || inputControllerB == null)
            {
                Debug.LogError("[SplitGridController] One or more dependencies are null in ApplyRoundConfig.");
                return;
            }

            Debug.Log($"[SplitGridController] Applying split config for round {config.roundNumber}");

            // Grid A setup
            gridA.ClearAllTiles();
            gridA.InitializeTileStates(config.gridAWidth, config.gridAHeight);
            gridA.SetTileElementService(tileElementServiceA);
            gridA.SetVirusService(virusA);
            gridA.SetDataFragmentService(dataFragmentServiceA);
            tileElementServiceA.ResizeGrid(config.gridAWidth, config.gridAHeight);
            dataFragmentServiceA.SpawnFragments(config.fragmentRequirementA);
            virusA.SpawnViruses(config.virusCountA, config.gridAWidth, config.gridAHeight);
            gridA.TriggerGridUpdate();

            // Grid B setup
            gridB.ClearAllTiles();
            gridB.InitializeTileStates(config.gridBWidth, config.gridBHeight);
            gridB.SetTileElementService(tileElementServiceB);
            gridB.SetVirusService(virusB);
            gridB.SetDataFragmentService(dataFragmentServiceB);
            tileElementServiceB.ResizeGrid(config.gridBWidth, config.gridBHeight);
            dataFragmentServiceB.SpawnFragments(config.fragmentRequirementB);
            virusB.SpawnViruses(config.virusCountB, config.gridBWidth, config.gridBHeight);
            gridB.TriggerGridUpdate();

            // Initialize views with correct input controllers
            gridViewA.Initialize(
                gridA,
                virusA,
                tileElementServiceA,
                inputControllerA,
                (x, y, btn) => splitGridInputController.HandleTileClick(x, y, GridID.A, btn),
                gridA.SymbolToolService
            );

            gridViewB.Initialize(
                gridB,
                virusB,
                tileElementServiceB,
                inputControllerB,
                (x, y, btn) => splitGridInputController.HandleTileClick(x, y, GridID.B, btn),
                gridB.SymbolToolService
            );

            if (progressTrackerService == null)
            {
                Debug.LogError("[SplitGridController] progressTrackerService is null!");
            }
            else
            {
                progressTrackerService.SetRequiredFragments(config.fragmentRequirementA + config.fragmentRequirementB);
            }

            progressTrackerService.ResetProgress();
            progressTrackerView.Initialize(progressTrackerService, gridA);

            // Ensure views are bound to their respective controllers
            inputControllerA.RebindView(gridViewA);
            inputControllerB.RebindView(gridViewB);
            
            // Set grid views on services
            gridA.SetGridView(gridViewA);
            gridB.SetGridView(gridViewB);

            // Initial grid refresh
            gridViewA.RefreshGrid(gridA);
            gridViewB.RefreshGrid(gridB);

            injectController.SetInjectButtonInteractable(true);

            Debug.Log($"[SplitGridController] GridA: {gridA.GridWidth}x{gridA.GridHeight}, empty positions: {gridA.GetValidInitialRevealPositions().Count}");
            Debug.Log($"[SplitGridController] GridB: {gridB.GridWidth}x{gridB.GridHeight}, empty positions: {gridB.GetValidInitialRevealPositions().Count}");

            Debug.Log("[SplitGridController] Finished applying split grid config");
        }
        public void ResetFlags() => flagManager.ResetFlags();
    }
}
