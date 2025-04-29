using UnityEngine;
using UnityEngine.UI;
using NewGameplay.Interfaces;
using NewGameplay.Services;
using System.Linq;

namespace NewGameplay.Controllers
{
    public class GridInputController : MonoBehaviour
    {
        [SerializeField] private GridViewNew view;
        private IGridService grid;
        private IInjectService inject;
        private IInjectService injectService;
        private bool hasPlacedFirstSymbol = false;
        public void Initialize(IGridService gridService, IInjectService injectService)
        {
            grid = gridService;
            inject = injectService;
            InjectServiceLocator.Service = inject;
            this.injectService = injectService;
        }

        public void HandleTileClick(int x, int y)
        {
            var selectedSymbol = injectService.SelectedSymbol;
            if (string.IsNullOrEmpty(selectedSymbol))
            {
                Debug.Log("[HandleTileClick] No symbol selected, ignoring click.");
                return;
            }

            // Before placement, check adjacency rules
            if (hasPlacedFirstSymbol)
            {
                if (!IsAdjacentToPlacedSymbol(x, y))
                {
                    Debug.Log("[HandleTileClick] Cannot place symbol here, not adjacent to existing symbols.");
                    return;
                }
            }
            
            grid.TryPlaceSymbol(x, y, selectedSymbol);
            grid.RevealTile(x, y);
            view.RefreshTileAt(x, y);
            injectService.ClearSelectedSymbol(selectedSymbol);
            
            var injectController = Object.FindFirstObjectByType<InjectController>();
            if (injectController != null)
            {
                injectController.RefreshUI();
            }

            hasPlacedFirstSymbol = true; // After successful placement, set the flag
        }
        private bool IsAdjacentToPlacedSymbol(int x, int y)
        {
            // Directions: up, down, left, right
            int[,] directions = new int[,]
            {
                { 0, 1 },
                { 0, -1 },
                { 1, 0 },
                { -1, 0 }
            };

            for (int i = 0; i < directions.GetLength(0); i++)
            {
                int checkX = x + directions[i, 0];
                int checkY = y + directions[i, 1];

                if (!grid.IsInBounds(checkX, checkY))
                    continue;

                string neighborSymbol = grid.GetSymbolAt(checkX, checkY);
                if (!string.IsNullOrEmpty(neighborSymbol))
                {
                    return true; // Found adjacent placed symbol
                }
            }

            return false;
        }
    }
}