using UnityEngine;
using NewGameplay.Interfaces;
using NewGameplay.Services;


namespace NewGameplay.Controllers
{
    public class ExtractController : MonoBehaviour
    {
        private IExtractService extractService;
        private IGridService gridService;
        private IProgressTrackerService progressService;
        [SerializeField] private EntropyTrackerView entropyTrackerView;
        [SerializeField] private ProgressTrackerView progressTrackerView;
        [SerializeField] private GridView gridView;

        public void Initialize(IExtractService extractService, IGridService gridService, IProgressTrackerService progressService)
        {
            this.extractService = extractService;
            this.gridService = gridService;
            this.progressService = progressService;
            extractService.onGridUpdated += RefreshGridView;
        }

        private void RefreshGridView()
        {
            gridView.RefreshGrid(gridService);
            entropyTrackerView.Refresh();
            progressTrackerView.Refresh();
        }

        // Called by EXTRACT button OnClick
        public void RunExtraction()
        {
            extractService.ExtractGrid();
            gridView.RefreshGrid(gridService);
            entropyTrackerView.Refresh();
            progressTrackerView.Refresh();
            //extractService.ClearProtectedTiles();
            FindFirstObjectByType<RoundManager>().CheckRoundEnd();
        }
    }
}
