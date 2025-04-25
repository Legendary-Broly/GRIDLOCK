using UnityEngine;
using UnityEngine.UI;
using NewGameplay.Interfaces;

namespace NewGameplay.Controllers
{
    public class GridInputController : MonoBehaviour
    {
        [SerializeField] private GridView view;
        private IGridService grid;
        private IInjectService inject;

        public void Initialize(IGridService gridService, IInjectService injectService)
        {
            grid = gridService;
            inject = injectService;
            InjectServiceLocator.Service = inject;
        }

        public void HandleTileClick(int x, int y)
        {
            grid.TryPlaceSymbol(x, y);
            view.RefreshGrid(grid);

            // Refresh the symbol bank UI after placement
            var injectController = Object.FindFirstObjectByType<InjectController>();
            if (injectController != null)
            {
                injectController.RefreshUI();
            }
        }
    }
}