using UnityEngine;
using NewGameplay.Interfaces;
using NewGameplay.Services;
using UnityEngine.UI;
using NewGameplay.Models;

namespace NewGameplay.Controllers
{
    public class ExtractController : MonoBehaviour
    {
        private IExtractService extractService;
        private IGridService gridService;
        private IProgressTrackerService progressService;
        private IDataFragmentService dataFragmentService;
        [SerializeField] private EntropyTrackerView entropyTrackerView;
        [SerializeField] private ProgressTrackerView progressTrackerView;
        [SerializeField] private GridViewNew gridView;
        [SerializeField] private RoundManager roundManager; // Optional direct reference
        // Inside ExtractController.cs

        [SerializeField] private Button extractButton; // Drag your ExtractButton here in Inspector

        private void Start()
        {
            if (extractButton != null)
            {
                extractButton.interactable = false; // Disable Extract at start
            }
        }

        private void Update()
        {
            if (dataFragmentService != null && dataFragmentService.IsFragmentFullySurrounded())
            {
                if (extractButton != null && !extractButton.interactable)
                {
                    extractButton.interactable = true;
                }
            }
        }

        public void Initialize(
            IExtractService extractService, 
            IGridService gridService, 
            IProgressTrackerService progressService,
            IDataFragmentService dataFragmentService)
        {
            this.extractService = extractService;
            this.gridService = gridService;
            this.progressService = progressService;
            this.dataFragmentService = dataFragmentService;

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
            
            // First try direct reference if available
            if (roundManager != null)
            {
                roundManager.CheckRoundEnd();
            }
            else
            {
                // Fall back to finding the object
                var foundRoundManager = Object.FindFirstObjectByType<RoundManager>();
                if (foundRoundManager != null)
                {
                    foundRoundManager.CheckRoundEnd();
                }
                else
                {
                    Debug.LogError("[ExtractController] RoundManager not found in the scene");
                }
            }
        }
    }
}
