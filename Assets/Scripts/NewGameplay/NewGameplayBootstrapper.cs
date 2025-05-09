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
using NewGameplay.Models;

namespace NewGameplay
{
    public class NewGameplayBootstrapper : MonoBehaviour
    {
        public GridService ExposedGridService { get; private set; }
        public TileElementService ExposedTileElementService { get; private set; }
        public IProgressTrackerService ExposedProgressService { get; private set; }
        public IDataFragmentService ExposedDataFragmentService => dataFragmentService;
        public ISystemIntegrityService ExposedSystemIntegrityService => systemIntegrityService;

        private IInjectService injectService;
        private GameOverController gameOverController;
        private CodeShardTrackerService codeShardTrackerService;
        private DataFragmentService dataFragmentService;
        private VirusSpawningStrategy virusSpawningStrategy;
        private VirusService virusService;
        private GridStateService gridStateService;
        private GridService gridService;
        private TileElementService tileElementService;
        private ProgressTrackerService progressService;
        private RoundService roundService;
        private ExtractService extractService;
        private SymbolToolService symbolToolService;
        private ISystemIntegrityService systemIntegrityService;

        [SerializeField] private GridInputController inputController;
        [SerializeField] private GridViewNew gridView;
        [SerializeField] private InjectController injectController;
        [SerializeField] private ExtractController extractController;
        [SerializeField] private ProgressTrackerView progressTrackerView;
        [SerializeField] private SystemIntegrityTrackerView systemIntegrityTrackerView;
        [SerializeField] private RoundPopupManager roundPopupManager;
        [SerializeField] private CSTrackerView csTrackerView;
        [SerializeField] private CompileButtonController compileButtonController;
        [SerializeField] private RoundManager roundManager;
        [SerializeField] private RoundConfigDatabase roundConfigDatabase;

        private void Awake()
        {
            gridStateService = new GridStateService();
            virusService = new VirusService(gridStateService);

            var tileElementConfigs = Resources.LoadAll<TileElementSO>("TileElements").ToList();
            tileElementService = new TileElementService(
                7, // Default grid size
                7, // Default grid size
                tileElementConfigs
            );

            gridService = new GridService(gridStateService, virusService);
            gridService.SetTileElementService(tileElementService);
            tileElementService.SetGridService(gridService);

            if (virusService is VirusService vs)
                vs.SetGridService(gridService);

            dataFragmentService = new DataFragmentService(gridService);
            dataFragmentService.SetGridView(gridView);
            dataFragmentService.SetTileElementService(tileElementService);

            gridService.SetGridView(gridView);
            progressService = new ProgressTrackerService(dataFragmentService);
            ExposedProgressService = progressService;
            gridService.SetProgressService(progressService);

            codeShardTrackerService = new CodeShardTrackerService();
            ExposedTileElementService = tileElementService;
            ExposedGridService = gridService;

            injectService = new InjectService();

            symbolToolService = new SymbolToolService(gridService, virusService);
            gridService.SetSymbolToolService(symbolToolService);
            injectService.SetSymbolToolService(symbolToolService);

            roundService = new RoundService(gridStateService, gridService, progressService, injectService, dataFragmentService, virusService, tileElementService);
            roundService.Initialize(roundConfigDatabase);

            extractService = new ExtractService(gridService, dataFragmentService);


            roundManager.Initialize(
                roundService,
                gridService,
                gridStateService,
                progressService,
                dataFragmentService,
                virusService,
                tileElementService
            );

            systemIntegrityService = new SystemIntegrityService();
            inputController.SetSystemIntegrityService(systemIntegrityService);

            gridView.Initialize(
                gridService,
                virusService,
                tileElementService,
                inputController,
                (x, y, button) => inputController.HandleTileClick(x, y, button),
                symbolToolService
            );
            
            roundService.ResetRound();
            tileElementService.ResizeGrid(gridService.GridWidth, gridService.GridHeight);

            roundManager.StartFirstRound();

            progressTrackerView.Initialize(progressService, gridService);
            systemIntegrityTrackerView.Initialize(systemIntegrityService);
            gridView.BuildGrid(gridService.GridWidth, gridService.GridHeight, (x, y, button) => inputController.HandleTileClick(x, y, button));
            tileElementService.ResizeGrid(gridService.GridWidth, gridService.GridHeight);
            gridView.RefreshGrid(gridService);

            inputController.Initialize(gridService, injectService, tileElementService, gridView, symbolToolService);
            inputController.SetVirusService(virusService);

            injectController.Initialize(injectService, gridService);
            extractController.Initialize(gridService, progressService, dataFragmentService, codeShardTrackerService, tileElementService, roundService, roundPopupManager);

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
