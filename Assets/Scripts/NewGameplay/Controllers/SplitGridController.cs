using UnityEngine;
using NewGameplay.Enums;
using NewGameplay.Services;
using NewGameplay.ScriptableObjects;
using NewGameplay.Controllers;
using NewGameplay.Data;
using NewGameplay.Interfaces;

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
            DataFragmentService fragmentB)
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

            Debug.Log($"[SplitGridController] Applying split config for round {config.roundNumber}");

            // Grid A
            gridA.ClearAllTiles();
            gridA.InitializeTileStates(config.gridAWidth, config.gridAHeight);
            tileElementServiceA.ResizeGrid(config.gridAWidth, config.gridAHeight);
            dataFragmentServiceA.SpawnFragments(config.fragmentRequirementA);
            virusA.SpawnViruses(config.virusCountA, config.gridAWidth, config.gridAHeight);

            // Grid B
            gridB.ClearAllTiles();
            gridB.InitializeTileStates(config.gridBWidth, config.gridBHeight);
            tileElementServiceB.ResizeGrid(config.gridBWidth, config.gridBHeight);
            dataFragmentServiceB.SpawnFragments(config.fragmentRequirementB);
            virusB.SpawnViruses(config.virusCountB, config.gridBWidth, config.gridBHeight);

            gridA.TriggerGridUpdate();
            gridB.TriggerGridUpdate();
        }

        public void ResetFlags() => flagManager.ResetFlags();
    }
}
