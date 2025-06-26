using UnityEngine;
using UnityEngine.EventSystems;
using System;
using NewGameplay.Interfaces;
using NewGameplay.Views;
using NewGameplay.Enums;

namespace NewGameplay.Controllers
{
    public class GridInputController : MonoBehaviour, IGridInputController
    {
        [SerializeField] private GridViewNew view;

        private IGridService gridService;
        private IInjectService injectService;
        private ITileElementService tileElementService;
        private ISymbolToolService symbolToolService;
        private IVirusService virusService;
        private ISystemIntegrityService systemIntegrityService;
        private IChatLogService chatLogService;
        private IPayloadService payloadService;
        private IDamageOverTimeService dotService;

        public event Action OnGridRendered;

        public void Initialize(
            IGridService gridService,
            IInjectService injectService,
            ITileElementService tileElementService,
            GridViewNew gridView,
            ISymbolToolService symbolToolService,
            IChatLogService chatLogService,
            IPayloadService payloadService,
            IDamageOverTimeService dotService)
        {
            this.gridService = gridService ?? throw new ArgumentNullException(nameof(gridService));
            this.injectService = injectService ?? throw new ArgumentNullException(nameof(injectService));
            this.tileElementService = tileElementService ?? throw new ArgumentNullException(nameof(tileElementService));
            this.view = gridView ?? throw new ArgumentNullException(nameof(gridView));
            this.symbolToolService = symbolToolService ?? throw new ArgumentNullException(nameof(symbolToolService));
            this.chatLogService = chatLogService ?? throw new ArgumentNullException(nameof(chatLogService));
            this.payloadService = payloadService ?? throw new ArgumentNullException(nameof(payloadService));
            this.dotService = dotService ?? throw new ArgumentNullException(nameof(dotService));

            WireUpEventHandlers();
        }

        private void WireUpEventHandlers()
        {
            if (symbolToolService != null)
            {
                symbolToolService.OnPivotActivated += HandlePivotStateChanged;
                symbolToolService.OnPivotDeactivated += HandlePivotStateChanged;
                symbolToolService.OnToolUsed += HandleToolUsed;
            }
        }

        private void HandlePivotStateChanged()
        {
            RenderGrid();
        }

        private void HandleToolUsed()
        {
            injectService?.RemoveSelectedTool();
        }

        public void SetVirusService(IVirusService virusService) => this.virusService = virusService ?? throw new ArgumentNullException(nameof(virusService));
        public void SetSystemIntegrityService(ISystemIntegrityService systemIntegrityService) => this.systemIntegrityService = systemIntegrityService ?? throw new ArgumentNullException(nameof(systemIntegrityService));
        public void RebindView(GridViewNew newView) => this.view = newView ?? throw new ArgumentNullException(nameof(newView));

        public void HandleTileClick(int x, int y)
        {
            HandleTileClick(x, y, PointerEventData.InputButton.Left);
        }

        public void HandleTileClick(int x, int y, PointerEventData.InputButton button)
        {
            switch (button)
            {
                case PointerEventData.InputButton.Right:
                    HandleRightClick(x, y);
                    break;
                case PointerEventData.InputButton.Left:
                    HandleLeftClick(x, y);
                    break;
            }
        }

        private void HandleRightClick(int x, int y)
        {
            if (!CanHandleRightClick(x, y)) return;

            bool isVirus = virusService.HasVirusAt(x, y);
            gridService.SetVirusFlag(x, y, isVirus);

            if (isVirus)
                chatLogService?.LogCorrectFlag();
            else
                chatLogService?.LogIncorrectFlag();

            gridService.DisableVirusFlag();
            RenderGrid();
        }

        private bool CanHandleRightClick(int x, int y)
        {
            if (!gridService.CanUseVirusFlag()) return false;

            bool isPivot = symbolToolService?.IsPivotActive() == true;
            var last = gridService.GetLastRevealedTile();
            
            if (!last.HasValue || gridService.IsTileRevealed(x, y)) return false;

            Vector2Int lastTile = last.Value;
            return isPivot
                ? Mathf.Abs(lastTile.x - x) == 1 && Mathf.Abs(lastTile.y - y) == 1
                : Mathf.Abs(lastTile.x - x) + Mathf.Abs(lastTile.y - y) == 1;
        }

        private void HandleLeftClick(int x, int y)
        {
            if (TryHandleToolPlacement(x, y)) return;
            if (!gridService.CanRevealTile(x, y)) return;

            HandleVirusReveal(x, y);
            HandleTileReveal(x, y);
        }

        private bool TryHandleToolPlacement(int x, int y)
        {
            string selectedTool = injectService?.GetSelectedTool();
            if (string.IsNullOrEmpty(selectedTool)) return false;

            symbolToolService?.TryPlaceSymbol(x, y, selectedTool);
            RenderGrid();
            return true;
        }

        private void HandleVirusReveal(int x, int y)
        {
            if (!virusService.HasVirusAt(x, y)) return;

            chatLogService?.LogVirusReveal();

            if (payloadService?.IsPayloadActive(PayloadType.DamageOverTime) == true)
                dotService?.AddDot(3, 25f);
            else
                systemIntegrityService?.Decrease(25f);
        }

        private void HandleTileReveal(int x, int y)
        {
            if (payloadService?.IsPayloadActive(PayloadType.DamageOverTime) == true)
                dotService?.TickDots();

            tileElementService?.OnTileRevealed(x, y);
            gridService.RevealTile(x, y);
            RenderGrid();
        }

        private void RenderGrid()
        {
            view.RenderGrid();
            OnGridRendered?.Invoke();
        }

        public void ActivatePivotToolAndRefreshGrid()
        {
            symbolToolService?.UsePivotTool();
            RenderGrid();
        }
    }
}
