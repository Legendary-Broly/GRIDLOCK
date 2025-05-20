using UnityEngine;
using NewGameplay.Interfaces;
using NewGameplay.Views;
using NewGameplay.Services;
using UnityEngine.EventSystems;
using NewGameplay.Controllers;
using System.Collections;
using System.Collections.Generic;

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

        public void SetPayloadManager(PayloadManager manager) => payloadManager = manager;

        public void Initialize(
            IGridService gridService,
            IInjectService injectService,
            ITileElementService tileElementService,
            GridViewNew gridView,
            SymbolToolService symbolToolService,
            IChatLogService chatLogService,
            PayloadManager payloadManager,
            IDamageOverTimeService dotService
        )
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
                symbolToolService.OnPivotActivated += OnPivotStateChanged;
                symbolToolService.OnPivotDeactivated += OnPivotStateChanged;
                symbolToolService.OnToolUsed += OnToolUsed;
            }

            InjectServiceLocator.Service = injectService;
        }

        private void OnToolUsed() => injectService?.RemoveSelectedTool();

        private void OnPivotStateChanged() => view.RefreshGrid(gridService);

        public void HandleTileClick(int x, int y, PointerEventData.InputButton button)
        {
            if (button == PointerEventData.InputButton.Right)
            {
                if (!gridService.CanUseVirusFlag()) return;

                bool isPivot = symbolToolService != null && symbolToolService.IsPivotActive();
                bool validFlagTarget = false;

                if (!gridService.IsTileRevealed(x, y) && gridService.GetLastRevealedTile().HasValue)
                {
                    var last = gridService.GetLastRevealedTile().Value;
                    validFlagTarget = isPivot
                        ? Mathf.Abs(last.x - x) == 1 && Mathf.Abs(last.y - y) == 1
                        : Mathf.Abs(last.x - x) + Mathf.Abs(last.y - y) == 1;
                }

                if (validFlagTarget)
                {
                    bool isVirus = virusService.HasVirusAt(x, y);
                    gridService.SetVirusFlag(x, y, isVirus);

                    if (isVirus)
                        chatLogService?.LogCorrectFlag();
                    else
                        chatLogService?.LogIncorrectFlag();

                    gridService.DisableVirusFlag();
                    view.RefreshTileAt(x, y);
                }

                return;
            }

            if (button != PointerEventData.InputButton.Left)
                return;

            // Use tool if selected
            string selectedTool = injectService?.GetSelectedTool();
            if (!string.IsNullOrEmpty(selectedTool))
            {
                symbolToolService?.TryPlaceSymbol(x, y, selectedTool);
                view.RefreshGrid(gridService);
                return;
            }

            if (!gridService.CanRevealTile(x, y))
            {
                Debug.Log($"[GridInputController] Cannot reveal ({x},{y}) – CanRevealTile==false");
                return;
            }

            // Virus Reveal → queue DoT or apply immediate damage
            if (virusService.HasVirusAt(x, y))
            {
                chatLogService?.LogVirusReveal();

                if (payloadManager != null && payloadManager.ShouldSpreadDamage())
                {
                    dotService?.AddDot(3, 25f); // ✅ Queue new damage instance
                }
                else
                {
                    systemIntegrityService?.Decrease(25f);
                }
            }

            // ✅ Always tick pending DoT effects after reveal
            if (payloadManager != null && payloadManager.ShouldSpreadDamage())
            {
                dotService?.TickDots();
            }

            tileElementService?.OnTileRevealed(x, y);
            gridService.RevealTile(x, y);
            view.RefreshGrid(gridService);
        }

        public void HandleTileClick(int x, int y)
        {
            HandleTileClick(x, y, PointerEventData.InputButton.Left);
        }

        public void SetVirusService(IVirusService virusService) => this.virusService = virusService;
        public void SetSystemIntegrityService(ISystemIntegrityService integrityService) => this.systemIntegrityService = integrityService;
        public void RebindView(GridViewNew newView) => this.view = newView;

        public void ActivatePivotToolAndRefreshGrid()
        {
            symbolToolService?.UsePivotTool();
            view.RefreshGrid(gridService);
        }
    }
}
