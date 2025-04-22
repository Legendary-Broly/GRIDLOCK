using UnityEngine;

public class NewGameplayBootstrapper : MonoBehaviour
{
    [SerializeField] private GridInputController inputController;
    [SerializeField] private GridView gridView;
    [SerializeField] private InjectController injectController;

    private void Awake()
    {
        var gridService = new GridService();
        var injectService = new InjectService();

        gridView.BuildGrid(gridService.GridSize, (x, y) => inputController.HandleTileClick(x, y));
        inputController.Initialize(gridService, injectService);
        injectController.Initialize(injectService, gridService);
    }
}
