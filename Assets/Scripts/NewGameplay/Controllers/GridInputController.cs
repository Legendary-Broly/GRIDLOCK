using UnityEngine;
using NewGameplay.Interfaces;
using NewGameplay.Views;
using NewGameplay.Services;
using UnityEngine.EventSystems;  // for SymbolPlacementService
using NewGameplay.Controllers;

namespace NewGameplay.Controllers
{
    public class GridInputController : MonoBehaviour
    {
        [SerializeField] private GridViewNew view;
        private IGridService gridService;
        private IInjectService injectService;
        private ITileElementService tileElementService;
        private SymbolPlacementService symbolPlacementService;
        private IVirusService virusService;
        public void Initialize(
            IGridService gridService,
            IInjectService injectService,
            ITileElementService tileElementService,
            GridViewNew gridView,
            SymbolPlacementService symbolPlacementService)
        {
            this.gridService = gridService;
            this.injectService = injectService;
            this.tileElementService = tileElementService;
            this.view = gridView;
            this.symbolPlacementService = symbolPlacementService;

            // make sure your injectService is globally accessible
            InjectServiceLocator.Service = injectService;
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

                if (!gridService.IsTileRevealed(x, y) &&
                    gridService.GetLastRevealedTile().HasValue &&
                    Mathf.Abs(gridService.GetLastRevealedTile().Value.x - x) + Mathf.Abs(gridService.GetLastRevealedTile().Value.y - y) == 1)
                {
                    if (virusService.HasVirusAt(x, y))
                    {
                        gridService.SetVirusFlag(x, y, true);
                        Debug.Log($"[GridInputController] ✅ Correct virus flag at ({x},{y})");
                        // TODO: reward logic here
                    }
                    else
                    {
                        Debug.Log($"[GridInputController] ❌ Incorrect virus flag at ({x},{y})");
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

            Debug.Log($"[GridInputController] Tile clicked at ({x},{y})");

            var selectedSymbol = injectService?.SelectedSymbol;
            if (!string.IsNullOrEmpty(selectedSymbol))
            {
                Debug.Log($"[GridInputController] Placing symbol '{selectedSymbol}' at ({x},{y})");
                symbolPlacementService.TryPlaceSymbol(x, y, selectedSymbol);
                injectService.ClearSelectedSymbol();
                FindFirstObjectByType<InjectController>()?.RefreshUI();
                view.RefreshGrid(gridService);
                return;
            }

            if (!gridService.CanRevealTile(x, y))
            {
                Debug.Log($"[GridInputController] Cannot reveal ({x},{y}) – CanRevealTile==false");
                return;
            }

            Debug.Log($"[GridInputController] Revealing tile at ({x},{y})");
            gridService.RevealTile(x, y);
            view.RefreshGrid(gridService);
        }

        public void RebindView(GridViewNew newView)
        {
            this.view = newView;
        }
        public void HandleTileClick(int x, int y)
        {
            HandleTileClick(x, y, PointerEventData.InputButton.Left);
        }
        public void SetVirusService(IVirusService virusService)
        {
            this.virusService = virusService;
        }
    }
}
