using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using NewGameplay.Interfaces;
using NewGameplay.Views;
using NewGameplay.Services;

namespace NewGameplay.Controllers
{
    public class GridInputController : MonoBehaviour
    {
        [SerializeField] private GridViewNew view;

        private IGridService gridService;
        private IInjectService injectService;
        private ITileElementService tileElementService;
        private SymbolToolService symbolToolService;
        private IVirusService virusService;
        private ISystemIntegrityService systemIntegrityService;
        private IChatLogService chatLogService;
        private PayloadManager payloadManager;
        private IDamageOverTimeService dotService;

        public void Initialize(
            IGridService gridService,
            IInjectService injectService,
            ITileElementService tileElementService,
            GridViewNew gridView,
            SymbolToolService symbolToolService,
            IChatLogService chatLogService,
            PayloadManager payloadManager,
            IDamageOverTimeService dotService)
        {
            this.gridService = gridService;
            this.injectService = injectService;
            this.tileElementService = tileElementService;
            this.view = gridView;
            this.symbolToolService = symbolToolService;
            this.chatLogService = chatLogService;
            this.payloadManager = payloadManager;
            this.dotService = dotService;

            if (symbolToolService != null)
            {
                symbolToolService.OnPivotActivated += RenderGrid;
                symbolToolService.OnPivotDeactivated += RenderGrid;
                symbolToolService.OnToolUsed += () => injectService?.RemoveSelectedTool();
            }

            InjectServiceLocator.Service = injectService;
        }

        public void SetVirusService(IVirusService virusService) => this.virusService = virusService;
        public void SetSystemIntegrityService(ISystemIntegrityService systemIntegrityService) => this.systemIntegrityService = systemIntegrityService;
        public void RebindView(GridViewNew newView) => this.view = newView;

        public void HandleTileClick(int x, int y)
        {
            HandleTileClick(x, y, PointerEventData.InputButton.Left);
        }

        public void HandleTileClick(int x, int y, PointerEventData.InputButton button)
        {
            if (button == PointerEventData.InputButton.Right)
            {
                HandleRightClick(x, y);
                return;
            }

            if (button != PointerEventData.InputButton.Left)
                return;

            HandleLeftClick(x, y);
        }

        private void HandleRightClick(int x, int y)
        {
            if (!gridService.CanUseVirusFlag()) return;

            bool isPivot = symbolToolService != null && symbolToolService.IsPivotActive();
            bool valid = false;

            var last = gridService.GetLastRevealedTile();
            if (last.HasValue && !gridService.IsTileRevealed(x, y))
            {
                Vector2Int lastTile = last.Value;
                valid = isPivot
                    ? Mathf.Abs(lastTile.x - x) == 1 && Mathf.Abs(lastTile.y - y) == 1
                    : Mathf.Abs(lastTile.x - x) + Mathf.Abs(lastTile.y - y) == 1;
            }

            if (!valid) return;

            bool isVirus = virusService.HasVirusAt(x, y);
            gridService.SetVirusFlag(x, y, isVirus);

            if (isVirus)
                chatLogService?.LogCorrectFlag();
            else
                chatLogService?.LogIncorrectFlag();

            gridService.DisableVirusFlag();
            RenderGrid();
        }

        private void HandleLeftClick(int x, int y)
        {
            string selectedTool = injectService?.GetSelectedTool();
            if (!string.IsNullOrEmpty(selectedTool))
            {
                symbolToolService?.TryPlaceSymbol(x, y, selectedTool);
                RenderGrid();
                return;
            }

            if (!gridService.CanRevealTile(x, y)) return;

            if (virusService.HasVirusAt(x, y))
            {
                chatLogService?.LogVirusReveal();

                if (payloadManager != null && payloadManager.ShouldSpreadDamage())
                    dotService?.AddDot(3, 25f);
                else
                    systemIntegrityService?.Decrease(25f);
            }

            if (payloadManager != null && payloadManager.ShouldSpreadDamage())
                dotService?.TickDots();

            tileElementService?.OnTileRevealed(x, y);
            gridService.RevealTile(x, y);
            RenderGrid();
        }

        private void RenderGrid()
        {
            view.RenderGrid();
        }

        public void ActivatePivotToolAndRefreshGrid()
        {
            symbolToolService?.UsePivotTool();
            RenderGrid();
        }
        public void SetPayloadManager(PayloadManager manager)
        {
            this.payloadManager = manager;
        }
    }
}
