using UnityEngine;
using UnityEngine.UI;

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
    }
}