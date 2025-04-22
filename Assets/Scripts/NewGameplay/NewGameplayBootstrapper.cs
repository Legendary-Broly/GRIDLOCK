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
    [SerializeField] private RoundManager roundManager;



    private void Awake()
    {
        var gridService = new GridService();
        var progressService = new ProgressTrackerService();
        var injectService = new InjectService();
        var roundService = new RoundService(gridService, progressService, injectService); // Pass required arguments
        var entropyService = new EntropyService();
        var extractService = new ExtractService(gridService, entropyService, progressService);

        roundManager.Initialize(roundService, progressService);
        progressTrackerView.Initialize(progressService);
        entropyTrackerView.Initialize(entropyService);
        ExposedGridService = gridService;
        gridView.BuildGrid(gridService.GridSize, (x, y) => inputController.HandleTileClick(x, y));
        inputController.Initialize(gridService, injectService);
        injectController.Initialize(injectService, gridService);
        extractController.Initialize(extractService, gridService);
        roundService.onRoundReset += () => 
        {
            gridView.RefreshGrid(gridService);  // Refresh grid visuals
            progressTrackerView.Refresh();      // Refresh progress display
        };
    }
}

