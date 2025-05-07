using UnityEngine;
using NewGameplay.Services;
using NewGameplay.Interfaces;
using NewGameplay.Controllers;
using System.Collections.Generic;
using System.Linq;
using NewGameplay.Enums;
using NewGameplay.Views;
using NewGameplay.UI;
using NewGameplay.Strategies;
using NewGameplay.ScriptableObjects;
using System;

namespace NewGameplay
{
    public class NewGameplayBootstrapper : MonoBehaviour
    {
        public GridService ExposedGridService { get; private set; }
        public TileElementService ExposedTileElementService { get; private set; }
        public IProgressTrackerService ExposedProgressService { get; private set; }
        public IDataFragmentService ExposedDataFragmentService => dataFragmentService;

        private IInjectService injectService;
        private GameOverController gameOverController;
        private CodeShardTrackerService codeShardTrackerService;
        private DataFragmentService dataFragmentService;
        public VirusSpawningStrategy ExposedVirusSpawningStrategy => virusSpawningStrategy;
        private VirusSpawningStrategy virusSpawningStrategy;
        private VirusService virusService;
        private GridStateService gridStateService;
        private SymbolPlacementService symbolPlacementService;
        private GridService gridService;
        private TileElementService tileElementService;
        private ProgressTrackerService progressService;
        private RoundService roundService;
        private ExtractService extractService;

        [SerializeField] private GridInputController inputController;
        [SerializeField] private GridViewNew gridView;
        [SerializeField] private InjectController injectController;
        [SerializeField] private ExtractController extractController;
        [SerializeField] private ProgressTrackerView progressTrackerView;
        [SerializeField] private RoundPopupManager roundPopupManager;
        [SerializeField] private CSTrackerView csTrackerView;
        [SerializeField] private CompileButtonController compileButtonController;
        [SerializeField] private RoundManager roundManager;
        [SerializeField] private RoundConfigDatabase roundConfigDatabase;

        private void Awake()
        {
            gridStateService = new GridStateService();
            symbolPlacementService = new SymbolPlacementService();
            virusService = new VirusService(gridStateService);

            gridService = new GridService(
                gridStateService,
                symbolPlacementService,
                null,
                virusService
            );

            if (virusService is VirusService vs)
            vs.SetGridService(gridService);
            
            symbolPlacementService.SetGridService(gridService);

            var tileElementConfigs = Resources.LoadAll<TileElementSO>("TileElements").ToList();
            tileElementService = new TileElementService(
                gridService.GridWidth,
                gridService.GridHeight,
                tileElementConfigs
            );

            // 🔧 RESIZE FIRST before anything uses the tile element grid
            tileElementService.SetGridService(gridService);

            dataFragmentService = new DataFragmentService(gridService);
            dataFragmentService.SetGridView(gridView);
            dataFragmentService.SetTileElementService(tileElementService);

            gridService.SetGridView(gridView);
            progressService = new ProgressTrackerService(dataFragmentService);
            ExposedProgressService = progressService;
            gridService.SetProgressService(progressService);

            var purgeEffectService = new PurgeEffectService(gridStateService, gridService);
            codeShardTrackerService = new CodeShardTrackerService();

            ExposedTileElementService = tileElementService;
            ExposedGridService = gridService;

            injectService = new InjectService();
            if (injectService is InjectService inject)
            {
                inject.SetGridService(gridService);
            }

            roundService = new RoundService(gridStateService, gridService, progressService, injectService, dataFragmentService, virusService, tileElementService);
            roundService.Initialize(roundConfigDatabase);

            extractService = new ExtractService(gridService, dataFragmentService);
            // 🟡 Manually trigger round logic BEFORE initializing roundManager
            roundService.ResetRound();

            roundManager.Initialize(
                roundService,
                gridService,
                gridStateService,
                progressService,
                dataFragmentService,
                virusService,
                tileElementService
            );

            gridView.Initialize(
                gridService,
                virusService, 
                tileElementService,
                inputController,
                (x, y, button) => inputController.HandleTileClick(x, y, button)
            );
            // No logic in StartFirstRound — UI only (optional)
            roundManager.StartFirstRound();

            progressTrackerView.Initialize(progressService, gridService);
            gridView.BuildGrid(gridService.GridWidth, gridService.GridHeight, (x, y, button) => inputController.HandleTileClick(x, y, button));
            Debug.Log($"[GridViewNew] Building grid: {gridService.GridWidth}x{gridService.GridHeight}");
            //gridService.InitializeTileStates(gridService.GridWidth, gridService.GridHeight);
            gridView.RefreshGrid(gridService);

            inputController.Initialize(gridService, injectService, tileElementService, gridView, symbolPlacementService);
            inputController.SetVirusService(virusService);

            injectController.Initialize(injectService, gridService);
            extractController.Initialize(extractService, gridService, progressService, dataFragmentService, codeShardTrackerService, tileElementService, roundService, roundPopupManager);

            gridService.SetGameOverController(gameOverController);
            csTrackerView.Initialize(codeShardTrackerService);
            compileButtonController.Initialize(codeShardTrackerService, injectService);

            roundService.onRoundReset += () =>
            {
                Debug.Log("[Bootstrapper] Round reset event received, refreshing UI...");
                gridView.RefreshGrid(gridService);
                progressTrackerView.Refresh();
                injectController.RefreshUI();
            };
        }
    }
}
