using UnityEngine;
using NewGameplay.Services;
using NewGameplay.Interfaces;
using NewGameplay.Controllers;

namespace NewGameplay
{
    public class NewGameplayBootstrapper : MonoBehaviour
    {
        public GridService ExposedGridService { get; private set; }
        public EntropyService ExposedEntropyService { get; private set; }

        [SerializeField] private GridInputController inputController;
        [SerializeField] private GridView gridView;
        [SerializeField] private InjectController injectController;
        [SerializeField] private ExtractController extractController;
        [SerializeField] private EntropyTrackerView entropyTrackerView;
        [SerializeField] private ProgressTrackerView progressTrackerView;
        [SerializeField] private RoundManager roundManager;
        [SerializeField] private RoundPopupController roundPopupController;
        [SerializeField] private MutationManager mutationManager;

        private void Awake()
        {
            // Create core services
            var entropyService = new EntropyService();
            var gridStateService = new GridStateService();
            var symbolPlacementService = new SymbolPlacementService(
                gridStateService,
                new PurgeEffectService(gridStateService, entropyService),
                new LoopEffectService(gridStateService)
            );
            var purgeEffectService = new PurgeEffectService(gridStateService, entropyService);
            var loopEffectService = new LoopEffectService(gridStateService);
            var virusSpreadService = new VirusSpreadService(gridStateService);

            // Create the grid service with all its dependencies
            var gridService = new GridService(
                gridStateService,
                symbolPlacementService,
                purgeEffectService,
                loopEffectService,
                virusSpreadService
            );

            var progressService = new ProgressTrackerService();
            IWeightedInjectService injectService = new WeightedInjectService();
            
            // Set the grid service in WeightedInjectService
            if (injectService is WeightedInjectService weightedInject)
            {
                weightedInject.SetGridService(gridService);
            }
            
            var roundService = new RoundService(gridService, progressService, injectService);
            var scoreService = new ScoreService();
            var extractService = new ExtractService(gridService, entropyService, scoreService, progressService);

            ExposedEntropyService = entropyService;
            ExposedGridService = gridService;

            // Create MutationEffectService
            var mutationEffectService = new MutationEffectService(entropyService, gridService, progressService);
            
            // Assign MutationEffectService to MutationManager
            mutationManager.SetMutationEffectService(mutationEffectService);

            // Set the entropy service in GridService
            gridService.SetEntropyService(entropyService);

            // Subscribe to entropy changes to update injection weights and show mutation panel
            entropyService.OnEntropyChanged += (float newValue, bool wasReset) => {
                injectService.UpdateWeights(newValue);
                // Also refresh the entropy view when entropy changes
                entropyTrackerView.Refresh();
                
                // Only show mutation panel when entropy resets from hitting 100%
                if (newValue == 0 && wasReset)
                {
                    mutationManager.ShowMutationPanel();
                }
            };

            roundManager.Initialize(roundService, progressService, roundPopupController);
            progressTrackerView.Initialize(progressService, gridService, entropyService);
            entropyTrackerView.Initialize(entropyService);
            gridView.BuildGrid(gridService.GridSize, (x, y) => inputController.HandleTileClick(x, y));
            inputController.Initialize(gridService, injectService);
            injectController.Initialize(injectService, gridService);
            extractController.Initialize(extractService, gridService, progressService);
            
            // Subscribe to round reset events
            roundService.onRoundReset += () => 
            {
                Debug.Log("[Bootstrapper] Round reset event received, refreshing UI...");
                gridView.RefreshGrid(gridService);  // Refresh grid visuals
                progressTrackerView.Refresh();      // Refresh progress display
                injectController.RefreshUI();       // Refresh symbol bank UI
            };
        }
    }
}

