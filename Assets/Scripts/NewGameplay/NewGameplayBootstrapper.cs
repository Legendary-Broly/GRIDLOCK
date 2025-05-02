using UnityEngine;
using NewGameplay.Services;
using NewGameplay.Interfaces;
using NewGameplay.Controllers;
using System.Collections.Generic;
using System.Linq;
using NewGameplay.Enums;
using NewGameplay.Views;
using NewGameplay.UI;

namespace NewGameplay
{
    public class NewGameplayBootstrapper : MonoBehaviour
    {
        public GridService ExposedGridService { get; private set; }
        public EntropyService ExposedEntropyService { get; private set; }
        public TileElementService ExposedTileElementService { get; private set; }
        public IProgressTrackerService ExposedProgressService { get; private set; }
        public IDataFragmentService ExposedDataFragmentService => dataFragmentService;
        [SerializeField] private SoundEffectService soundService;
        public SoundEffectService ExposedSoundService => soundService;

        private RoundManager roundManager;
        private IWeightedInjectService weightedInjectService;
        private GameOverController gameOverController;
        private CodeShardTrackerService codeShardTrackerService;
        private DataFragmentService dataFragmentService;

        [SerializeField] private GridInputController inputController;
        [SerializeField] private GridViewNew gridView;
        [SerializeField] private InjectController injectController;
        [SerializeField] private ExtractController extractController;
        [SerializeField] private EntropyTrackerView entropyTrackerView;
        [SerializeField] private ProgressTrackerView progressTrackerView;
        [SerializeField] private RoundPopupManager roundPopupManager;
        [SerializeField] private CSTrackerView csTrackerView;
        [SerializeField] private CompileButtonController compileButtonController;

        private void Awake()
        {
            var entropyService = new EntropyService();
            var gridStateService = new GridStateService();
            var symbolPlacementService = new SymbolPlacementService();
            var virusSpreadService = new VirusSpreadService(gridStateService);

            var gridService = new GridService(
                gridStateService,
                symbolPlacementService,
                null,
                virusSpreadService
            );
            symbolPlacementService.SetGridService(gridService);

            // Tile Element Service setup (must come before dataFragmentService)
            var tileElementConfigs = Resources.LoadAll<TileElementSO>("TileElements").ToList();
            var tileElementService = new TileElementService(
                gridService.GridWidth,
                gridService.GridHeight,
                tileElementConfigs,
                entropyService
            );
            tileElementService.GenerateElements();
            tileElementService.SetGridService(gridService);

            dataFragmentService = new DataFragmentService(gridService);
            dataFragmentService.SetGridView(gridView);
            dataFragmentService.SetTileElementService(tileElementService);

            gridService.SetGridView(gridView);
            var progressService = new ProgressTrackerService(dataFragmentService);
            ExposedProgressService = progressService;
            gridService.SetProgressService(progressService);

            var purgeEffectService = new PurgeEffectService(gridStateService, gridService);
            codeShardTrackerService = new CodeShardTrackerService();

            gridService.SetEntropyService(entropyService);
            gridService.SetTileElementService(tileElementService);

            ExposedTileElementService = tileElementService;
            ExposedEntropyService = entropyService;
            ExposedGridService = gridService;

            IWeightedInjectService injectService = new WeightedInjectService();
            if (injectService is WeightedInjectService weightedInject)
            {
                weightedInject.SetGridService(gridService);
                weightedInjectService = weightedInject;
            }

            var roundService = new RoundService(gridService, progressService, injectService, dataFragmentService, virusSpreadService);
            var extractService = new ExtractService(gridService, entropyService, dataFragmentService);

            roundManager = new RoundManager(progressService, dataFragmentService, roundPopupManager, roundService, gridService, gridView);
            roundManager.StartFirstRound();

            progressTrackerView.Initialize(progressService, gridService, entropyService);
            entropyTrackerView.Initialize(entropyService);
            gridView.Initialize(gridService, tileElementService, virusSpreadService);
            gridView.BuildGrid(gridService.GridWidth, gridService.GridHeight, (x, y) => inputController.HandleTileClick(x, y));
            gridService.InitializeTileStates(gridService.GridWidth, gridService.GridHeight);
            gridView.RefreshGrid(gridService);

            inputController.Initialize(gridService, injectService, tileElementService, entropyService, gridView, symbolPlacementService);
            injectController.Initialize(injectService, gridService);
            extractController.Initialize(extractService, gridService, progressService, dataFragmentService, codeShardTrackerService, tileElementService, roundManager);

            gridService.SetGameOverController(gameOverController);
            csTrackerView.Initialize(codeShardTrackerService);
            compileButtonController.Initialize(codeShardTrackerService, weightedInjectService);

            entropyService.OnEntropyChanged += (float newValue, bool wasReset) => {
                injectService.UpdateWeights(newValue);
                entropyTrackerView.Refresh();

                // Play sound effects based on entropy changes
                if (soundService != null && !wasReset) // Don't play sounds on reset
                {
                    float oldValue = entropyService.EntropyPercent;
                    if (newValue > oldValue)
                    {
                        soundService.PlayEntropyGain();
                    }
                    else if (newValue < oldValue)
                    {
                        soundService.PlayEntropyLoss();
                    }
                }
            };

            roundService.onRoundReset += () => {
                Debug.Log("[Bootstrapper] Round reset event received, refreshing UI...");
                gridView.RefreshGrid(gridService);
                progressTrackerView.Refresh();
                injectController.RefreshUI();
            };
        }
    }
}
