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
        public MutationEffectService ExposedMutationEffectService { get; private set; }

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
            Debug.Log("[NewGameplayBootstrapper] Awake - Starting initialization");
            
            // Create core services
            var entropyService = new EntropyService();
            var gridStateService = new GridStateService();
            
            // Create MutationEffectService early so it can be injected into other services
            var progressService = new ProgressTrackerService();

            Debug.Log("[NewGameplayBootstrapper] Creating MutationEffectService with null gridService (will set later)");
            var mutationEffectService = new MutationEffectService(
                entropyService, 
                null, // Will set this circular reference later
                progressService
            );

            Debug.Log("[NewGameplayBootstrapper] Creating dependent services");
            var purgeEffectService = new PurgeEffectService(gridStateService, entropyService);
            var loopEffectService = new LoopEffectService(gridStateService, mutationEffectService);
            
            // Create SymbolPlacementService with mutation and entropy dependencies
            var symbolPlacementService = new SymbolPlacementService(
                gridStateService,
                purgeEffectService,
                loopEffectService,
                mutationEffectService,
                entropyService
            );
            
            var virusSpreadService = new VirusSpreadService(gridStateService);

            // Create the grid service with all its dependencies
            Debug.Log("[NewGameplayBootstrapper] Creating GridService");
            var gridService = new GridService(
                gridStateService,
                symbolPlacementService,
                purgeEffectService,
                loopEffectService,
                virusSpreadService
            );
            
            // Set the grid service in MutationEffectService to resolve circular reference
            Debug.Log("[NewGameplayBootstrapper] Setting GridService in MutationEffectService");
            mutationEffectService.SetGridService(gridService);

            IWeightedInjectService injectService = new WeightedInjectService();
            
            // Set the grid service in WeightedInjectService
            if (injectService is WeightedInjectService weightedInject)
            {
                weightedInject.SetGridService(gridService);
            }
            
            var roundService = new RoundService(gridService, progressService, injectService);
            
            // Create ScoreService with MutationEffectService
            var scoreService = new ScoreService(mutationEffectService);
            var extractService = new ExtractService(gridService, entropyService, scoreService, progressService);
            
            // Set the MutationEffectService in SymbolEffectProcessor
            NewGameplay.Utility.SymbolEffectProcessor.SetMutationEffectService(mutationEffectService);

            // Expose services to other scripts
            Debug.Log("[NewGameplayBootstrapper] Exposing services for external access");
            ExposedEntropyService = entropyService;
            ExposedGridService = gridService;
            ExposedMutationEffectService = mutationEffectService;
            
            // Assign MutationEffectService to MutationManager
            Debug.Log("[NewGameplayBootstrapper] Setting MutationEffectService in MutationManager");
            mutationManager.SetMutationEffectService(mutationEffectService);

            // Set the mutation service on PurgeEffectService
            purgeEffectService.SetMutationEffectService(mutationEffectService);

            // Set the entropy service in GridService
            Debug.Log("[NewGameplayBootstrapper] Setting EntropyService in GridService");
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
            
            Debug.Log("[NewGameplayBootstrapper] Initializing UI controllers");
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
            
            Debug.Log("[NewGameplayBootstrapper] Initialization complete");
        }
        
        // Make sure our references are valid when enabled
        private void OnEnable()
        {
            Debug.Log("[NewGameplayBootstrapper] OnEnable - Checking service references");
            if (ExposedEntropyService == null)
            {
                Debug.LogError("[NewGameplayBootstrapper] ExposedEntropyService is null in OnEnable!");
            }
            
            if (ExposedGridService == null)
            {
                Debug.LogError("[NewGameplayBootstrapper] ExposedGridService is null in OnEnable!");
            }
            
            if (ExposedMutationEffectService == null)
            {
                Debug.LogError("[NewGameplayBootstrapper] ExposedMutationEffectService is null in OnEnable!");
            }
        }
    }
}

