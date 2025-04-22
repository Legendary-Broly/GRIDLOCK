using UnityEngine;

public class ExtractController : MonoBehaviour
{
    private IExtractService extractService;
    [SerializeField] private EntropyTrackerView entropyTrackerView;
    [SerializeField] private ProgressTrackerView progressTrackerView;
    [SerializeField] private GridView gridView;
    private IGridService gridService;

    public void Initialize(IExtractService extractService, IGridService gridService)
    {
        this.extractService = extractService;
        this.gridService = gridService;
        extractService.onGridUpdated += () => gridView.RefreshGrid(gridService);

    }

    // Called by EXTRACT button OnClick
    public void RunExtraction()
    {
        extractService.ExtractGrid();
        gridView.RefreshGrid(gridService);  // Keep refresh here
        entropyTrackerView.Refresh();
        progressTrackerView.Refresh();
        extractService.ClearProtectedTiles();  // ADD THIS LINE (new method)
        FindFirstObjectByType<RoundManager>().CheckRoundEnd();
    }
}
