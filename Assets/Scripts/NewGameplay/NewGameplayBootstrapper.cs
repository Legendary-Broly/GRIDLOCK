using UnityEngine;
using NewGameplay.Services;
using NewGameplay.Interfaces;
using NewGameplay.Controllers;
using System.Collections.Generic;
using System.Linq;
using NewGameplay.Enums;

namespace NewGameplay
{
    public class NewGameplayBootstrapper : MonoBehaviour
    {
        public GridService ExposedGridService { get; private set; }
        public EntropyService ExposedEntropyService { get; private set; }

        [SerializeField] private GridInputController inputController;
        [SerializeField] private GridViewNew gridView;
        [SerializeField] private InjectController injectController;
        [SerializeField] private ExtractController extractController;
        [SerializeField] private EntropyTrackerView entropyTrackerView;
        [SerializeField] private ProgressTrackerView progressTrackerView;
        [SerializeField] private RoundManager roundManager;
        [SerializeField] private RoundPopupController roundPopupController;
        [SerializeField] private MutationManager mutationManager;

        private void Awake()
        {
            // === Core Services ===
            var entropyService = new EntropyService();
            var gridStateService = new GridStateService();

            // === GridService constructed early so it can be passed to dependent services
            var symbolPlacementService = new SymbolPlacementService(
                gridStateService,
                null, // Will set purgeEffectService later
                null, // Will set mutationEffectService later
                entropyService
            );

            var virusSpreadService = new VirusSpreadService(gridStateService);

            var gridService = new GridService(
                gridStateService,
                symbolPlacementService,
                null, // Will set purgeEffectService later
                virusSpreadService
            );
            symbolPlacementService.SetGridService(gridService);

            var dataFragmentService = new DataFragmentService(gridService);
            dataFragmentService.SetGridView(gridView);
            gridService.SetGridView(gridView);

            var progressService = new ProgressTrackerService(dataFragmentService);
            var mutationEffectService = new MutationEffectService(entropyService, gridService, progressService);

            var purgeEffectService = new PurgeEffectService(gridStateService, gridService);

            // === Wire up circular references ===
            symbolPlacementService.SetVirusSpreadService(virusSpreadService);
            symbolPlacementService.SetPurgeEffectService(purgeEffectService);
            symbolPlacementService.SetGridView(gridView);
            symbolPlacementService.SetTileElementService(null); // Will assign below
            mutationEffectService.SetGridService(gridService);
            gridService.SetEntropyService(entropyService);

            // === Tile Elements ===
            var tileElementConfigs = Resources.LoadAll<TileElementSO>("TileElements").ToList();
            var tileElementService = new TileElementService(
                gridService.GridWidth,
                gridService.GridHeight,
                tileElementConfigs,
                progressService,
                entropyService
            );
            tileElementService.GenerateElements();
            symbolPlacementService.SetTileElementService(tileElementService);

            // === Injection and Extraction ===
            IWeightedInjectService injectService = new WeightedInjectService();
            if (injectService is WeightedInjectService weightedInject)
            {
                weightedInject.SetGridService(gridService);
            }

            var roundService = new RoundService(gridService, progressService, injectService, dataFragmentService);
            var scoreService = new ScoreService(mutationEffectService);
            var extractService = new ExtractService(gridService, entropyService, scoreService, progressService, dataFragmentService);

            // === Scene Wireup ===
            ExposedEntropyService = entropyService;
            ExposedGridService = gridService;

            roundManager.Initialize(roundService, progressService, roundPopupController, dataFragmentService);
            progressTrackerView.Initialize(progressService, gridService, entropyService);
            entropyTrackerView.Initialize(entropyService);
            gridView.Initialize(gridService, tileElementService);
            gridView.BuildGrid(gridService.GridWidth, gridService.GridHeight, (x, y) => inputController.HandleTileClick(x, y));
            gridService.InitializeTileStates(gridService.GridWidth, gridService.GridHeight);
            inputController.Initialize(gridService, injectService);
            injectController.Initialize(injectService, gridService);
            extractController.Initialize(extractService, gridService, progressService, dataFragmentService);

            // === Entropy Events ===
            entropyService.OnEntropyChanged += (float newValue, bool wasReset) => {
                injectService.UpdateWeights(newValue);
                entropyTrackerView.Refresh();
                if (newValue == 0 && wasReset)
                {
                    mutationManager.ShowMutationPanel();
                }
            };

            // === Progress → Fragment Trigger ===
            progressService.OnProgressGoalReached += () =>
            {
                if (!dataFragmentService.GetFragmentPosition().HasValue)
                {
                    dataFragmentService.SpawnFragment();
                }
            };

            // === Round Reset Hook ===
            roundService.onRoundReset += () => {
                Debug.Log("[Bootstrapper] Round reset event received, refreshing UI...");
                gridView.RefreshGrid(gridService);
                progressTrackerView.Refresh();
                injectController.RefreshUI();
            };
        }
    }
}
