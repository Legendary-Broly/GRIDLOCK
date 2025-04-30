using UnityEngine;
using UnityEngine.UI;
using NewGameplay.Interfaces;
using NewGameplay.Services;
using System.Linq;
using NewGameplay.Models;
namespace NewGameplay.Controllers
{
    public class GridInputController : MonoBehaviour
    {
        [SerializeField] private GridViewNew view;
        private IGridService grid;
        private IInjectService inject;
        private IInjectService injectService;
        //private bool hasPlacedFirstSymbol = false;
        private ITileElementService tileElementService;
        private IEntropyService entropyService;
        private SymbolPlacementService symbolPlacementService;
        public void Initialize(
            IGridService gridService,
            IInjectService injectService,
            ITileElementService tileElementService,
            IEntropyService entropyService,
            GridViewNew gridView,
            SymbolPlacementService symbolPlacementService)
        {
            this.grid = gridService;
            this.injectService = injectService;
            this.tileElementService = tileElementService;
            this.entropyService = entropyService;
            this.view = gridView;
            this.symbolPlacementService = symbolPlacementService;

            InjectServiceLocator.Service = injectService;
        }

        public void HandleTileClick(int x, int y)
        {
            string selectedSymbol = injectService?.SelectedSymbol;

            // Handle symbol placement first
            if (!string.IsNullOrEmpty(selectedSymbol))
            {
                Debug.Log($"[GridInput] Attempting to place symbol '{selectedSymbol}' at ({x},{y})");
                symbolPlacementService?.TryPlaceSymbol(x, y, selectedSymbol);
                injectService.ClearSelectedSymbol(selectedSymbol);
                var injectController = FindFirstObjectByType<InjectController>();
                injectController?.RefreshUI();
                view.RefreshGrid(grid);
                return;  // Return here to prevent default tile reveal
            }

            // Default tile reveal (only if no symbol was placed)
            grid.RevealTile(x, y);
            view.RefreshGrid(grid);

            string symbol = grid.GetSymbolAt(x, y);
            if (symbol == "X")
            {
                Debug.Log($"[GridInput] Revealed a virus at ({x},{y})! Entropy increased.");
                entropyService.Increase(10);
            }
        }
    }
}