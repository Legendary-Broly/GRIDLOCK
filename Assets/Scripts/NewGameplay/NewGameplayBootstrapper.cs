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
            var entropyService = new EntropyService();
            var gridStateService = new GridStateService();
            var progressService = new ProgressTrackerService();

            var mutationEffectService = new MutationEffectService(
                entropyService, 
                null, // Will set this circular reference later
                progressService
            );

            var purgeEffectService = new PurgeEffectService(gridStateService, entropyService);
            var loopEffectService = new LoopEffectService(gridStateService, mutationEffectService);
            var symbolPlacementService = new SymbolPlacementService(
                gridStateService,
                purgeEffectService,
                loopEffectService,
                mutationEffectService,
                entropyService
            );

            var virusSpreadService = new VirusSpreadService(gridStateService);
            var gridService = new GridService(
                gridStateService,
                symbolPlacementService,
                purgeEffectService,
                loopEffectService,
                virusSpreadService
            );

            mutationEffectService.SetGridService(gridService);

            var dataFragmentService = new DataFragmentService(gridService);

            IWeightedInjectService injectService = new WeightedInjectService();
            if (injectService is WeightedInjectService weightedInject)
            {
                weightedInject.SetGridService(gridService);
            }

            var roundService = new RoundService(gridService, progressService, injectService, dataFragmentService);
            var scoreService = new ScoreService(mutationEffectService);
            var extractService = new ExtractService(gridService, entropyService, scoreService, progressService, dataFragmentService);

            NewGameplay.Utility.SymbolEffectProcessor.SetMutationEffectService(mutationEffectService);

            ExposedEntropyService = entropyService;
            ExposedGridService = gridService;
            ExposedMutationEffectService = mutationEffectService;

            mutationManager.SetMutationEffectService(mutationEffectService);
            purgeEffectService.SetMutationEffectService(mutationEffectService);
            gridService.SetEntropyService(entropyService);

            entropyService.OnEntropyChanged += (float newValue, bool wasReset) => {
                injectService.UpdateWeights(newValue);
                entropyTrackerView.Refresh();
                if (newValue == 0 && wasReset)
                {
                    mutationManager.ShowMutationPanel();
                }
            };

            roundManager.Initialize(roundService, progressService, roundPopupController, dataFragmentService);
            progressTrackerView.Initialize(progressService, gridService, entropyService);
            entropyTrackerView.Initialize(entropyService);
            gridView.BuildGrid(gridService.GridWidth, (x, y) => inputController.HandleTileClick(x, y));
            inputController.Initialize(gridService, injectService);
            injectController.Initialize(injectService, gridService);
            extractController.Initialize(extractService, gridService, progressService);

            roundService.onRoundReset += () => 
            {
                Debug.Log("[Bootstrapper] Round reset event received, refreshing UI...");
                gridView.RefreshGrid(gridService);
                progressTrackerView.Refresh();
                injectController.RefreshUI();
            };
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

