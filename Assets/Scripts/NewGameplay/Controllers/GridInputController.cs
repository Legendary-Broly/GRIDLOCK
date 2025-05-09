using UnityEngine;
using NewGameplay.Interfaces;
using NewGameplay.Views;
using NewGameplay.Services;
using UnityEngine.EventSystems;
using NewGameplay.Controllers;

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

        public void Initialize(
            IGridService gridService,
            IInjectService injectService,
            ITileElementService tileElementService,
            GridViewNew gridView,
            SymbolToolService symbolToolService)
        {
            this.gridService = gridService;
            this.injectService = injectService;
            this.tileElementService = tileElementService;
            this.view = gridView;
            this.symbolToolService = symbolToolService;

            if (symbolToolService != null)
            {
                symbolToolService.OnPivotActivated += OnPivotStateChanged;
                symbolToolService.OnPivotDeactivated += OnPivotStateChanged;
                symbolToolService.OnToolUsed += OnToolUsed;
            }

            InjectServiceLocator.Service = injectService;
        }

        private void OnToolUsed()
        {
            injectService?.RemoveSelectedTool();
        }

        private void OnPivotStateChanged()
        {
            view.RefreshGrid(gridService);
        }

        public void HandleTileClick(int x, int y, PointerEventData.InputButton button)
        {
            Debug.Log($"[GridInputController] Tile click received at ({x},{y}) with button {button}");

            if (button == PointerEventData.InputButton.Right)
            {
                if (!gridService.CanUseVirusFlag())
                {
                    Debug.Log($"[GridInputController] Right-click ignored — virus flag already used.");
                    return;
                }

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
                    {
                        Debug.Log($"[GridInputController] ✅ Correct virus flag at ({x},{y})");
                        // Optional: reward logic
                    }
                    else
                    {
                        Debug.Log($"[GridInputController] ❌ Incorrect virus flag at ({x},{y})");
                        systemIntegrityService?.Decrease(5f);
                    }

                    gridService.DisableVirusFlag();
                    view.RefreshTileAt(x, y);
                }
                else
                {
                    Debug.Log($"[GridInputController] Invalid right-click target at ({x},{y})");
                }

                return;
            }

            if (button != PointerEventData.InputButton.Left)
                return;

            // Check if a tool is selected
            string selectedTool = injectService?.GetSelectedTool();
            if (!string.IsNullOrEmpty(selectedTool))
            {
                Debug.Log($"[GridInputController] Using tool {selectedTool} at ({x},{y})");
                symbolToolService?.TryPlaceSymbol(x, y, selectedTool);
                view.RefreshGrid(gridService);
                return;
            }

            if (!gridService.CanRevealTile(x, y))
            {
                Debug.Log($"[GridInputController] Cannot reveal ({x},{y}) – CanRevealTile==false");
                return;
            }

            Debug.Log($"[GridInputController] Revealing tile at ({x},{y})");

            // Check if the tile has a virus before revealing
            if (virusService.HasVirusAt(x, y))
            {
                systemIntegrityService?.Decrease(25f);
            }

            gridService.RevealTile(x, y);
            view.RefreshGrid(gridService);
        }

        public void HandleTileClick(int x, int y)
        {
            HandleTileClick(x, y, PointerEventData.InputButton.Left);
        }

        public void SetVirusService(IVirusService virusService)
        {
            this.virusService = virusService;
        }

        public void SetSystemIntegrityService(ISystemIntegrityService integrityService)
        {
            this.systemIntegrityService = integrityService;
        }

        public void RebindView(GridViewNew newView)
        {
            this.view = newView;
        }

        public void ActivatePivotToolAndRefreshGrid()
        {
            symbolToolService?.UsePivotTool();
            view.RefreshGrid(gridService);
        }
    }
}
