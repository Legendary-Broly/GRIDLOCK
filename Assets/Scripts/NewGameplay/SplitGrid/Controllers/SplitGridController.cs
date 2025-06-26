using UnityEngine;
using NewGameplay.Interfaces;
using NewGameplay.ScriptableObjects;
using NewGameplay.Views;
using NewGameplay.Services;
using NewGameplay.SplitGrid.Interfaces;
using NewGameplay.SplitGrid.Services;
using NewGameplay.SplitGrid.Views;
using NewGameplay.SplitGrid.Data;
using NewGameplay.SplitGrid.Controllers;


namespace NewGameplay.SplitGrid.Controllers
{
    public class SplitGridController : MonoBehaviour
    {
        [SerializeField] private GridViewNew gridViewA;
        [SerializeField] private GridViewNew gridViewB;

        private ISplitGridService splitGridService;
        private ITileElementService tileElementServiceA;
        private ITileElementService tileElementServiceB;
        private ISymbolToolService symbolToolServiceA;
        private ISymbolToolService symbolToolServiceB;
        private IVirusService virusServiceA;
        private IVirusService virusServiceB;
        private IChatLogService chatLogService;

        public void Initialize(
            ISplitGridService splitGridService,
            ITileElementService tileElementServiceA,
            ITileElementService tileElementServiceB,
            ISymbolToolService symbolToolServiceA,
            ISymbolToolService symbolToolServiceB,
            IVirusService virusServiceA,
            IVirusService virusServiceB,
            IChatLogService chatLogService)
        {
            this.splitGridService = splitGridService;
            this.tileElementServiceA = tileElementServiceA;
            this.tileElementServiceB = tileElementServiceB;
            this.symbolToolServiceA = symbolToolServiceA;
            this.symbolToolServiceB = symbolToolServiceB;
            this.virusServiceA = virusServiceA;
            this.virusServiceB = virusServiceB;
            this.chatLogService = chatLogService;

            // Bind services to grids
            splitGridService.SetTileElementService(GridID.GridA, tileElementServiceA);
            splitGridService.SetTileElementService(GridID.GridB, tileElementServiceB);
            splitGridService.SetSymbolToolService(GridID.GridA, symbolToolServiceA);
            splitGridService.SetSymbolToolService(GridID.GridB, symbolToolServiceB);

            // Subscribe to grid updates
            splitGridService.OnGridsUpdated += HandleGridsUpdated;
        }

        public void ApplySplitRoundConfig(RoundConfigSO config)
        {
            if (config == null) return;

            // Initialize both grids with the same dimensions
            splitGridService.InitializeGrids(config.gridWidth, config.gridHeight);

            // Clear existing state
            splitGridService.ClearAllGrids();

            // Lock interaction until first reveal
            splitGridService.LockInteraction(GridID.GridA);
            splitGridService.LockInteraction(GridID.GridB);

            // Trigger view updates
            HandleGridsUpdated();
        }

        public void HandleTileClick(GridID gridId, int x, int y, bool isRightClick = false)
        {
            if (!splitGridService.IsInBounds(gridId, x, y)) return;

            if (isRightClick)
            {
                HandleRightClick(gridId, x, y);
                return;
            }

            HandleLeftClick(gridId, x, y);
        }

        private void HandleLeftClick(GridID gridId, int x, int y)
        {
            if (!splitGridService.CanRevealTile(gridId, x, y)) return;

            splitGridService.RevealTile(gridId, x, y);
        }

        private void HandleRightClick(GridID gridId, int x, int y)
        {
            if (!splitGridService.CanUseVirusFlag(gridId)) return;
            splitGridService.HandleVirusFlag(gridId, x, y, chatLogService);
        }

        private void HandleGridsUpdated()
        {
            // Update both grid views
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