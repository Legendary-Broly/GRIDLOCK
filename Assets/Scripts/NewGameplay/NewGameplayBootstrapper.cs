using UnityEngine;

public class NewGameplayBootstrapper : MonoBehaviour
{
    public GridService ExposedGridService { get; private set; }

    [SerializeField] private GridInputController inputController;
    [SerializeField] private GridView gridView;
    [SerializeField] private InjectController injectController;
    [SerializeField] private ExtractController extractController;
    [SerializeField] private EntropyTrackerView entropyTrackerView;
    [SerializeField] private ProgressTrackerView progressTrackerView;


    private void Awake()
    {
        var entropyService = new EntropyService();
        var progressService = new ProgressTrackerService();
        var gridService = new GridService();
        var injectService = new InjectService();
        var extractService = new ExtractService(gridService, entropyService, progressService);
        
        progressTrackerView.Initialize(progressService);
        entropyTrackerView.Initialize(entropyService);
        ExposedGridService = gridService;
        gridView.BuildGrid(gridService.GridSize, (x, y) => inputController.HandleTileClick(x, y));
        inputController.Initialize(gridService, injectService);
        injectController.Initialize(injectService, gridService);
        extractController.Initialize(extractService, gridService); // ✅ include the missing argument
    }
}

