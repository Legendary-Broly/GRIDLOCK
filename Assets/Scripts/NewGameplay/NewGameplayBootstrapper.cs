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
using System.Collections;

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
        private IChatLogService chatLogService;
        private DebugController debugController;
        private IDamageOverTimeService dotService;
        [SerializeField] private GridInputController inputController;
        [SerializeField] private GridViewNew gridView;
        [SerializeField] private InjectController injectController;
        [SerializeField] private ExtractController extractController;
        [SerializeField] private ProgressTrackerView progressTrackerView;
        [SerializeField] private SystemIntegrityTrackerView systemIntegrityTrackerView;
        [SerializeField] private RoundPopupManager roundPopupManager;
        [SerializeField] private CSTrackerView csTrackerView;
        //[SerializeField] private CompileButtonController compileButtonController;
        [SerializeField] private RoundManager roundManager;
        [SerializeField] private RoundConfigDatabase roundConfigDatabase;
        [SerializeField] private GameOverController gameOverController;
        [SerializeField] private ChatLogView chatLogView;
        [SerializeField] private PayloadManager payloadManager;

        
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

            gridService = new GridService(gridStateService, virusService, chatLogService);
            gridService.SetTileElementService(tileElementService);
            tileElementService.SetGridService(gridService);
            tileElementService.SetGameRunner(this);

            if (virusService is VirusService vs)
                vs.SetGridService(gridService);

            dataFragmentService = new DataFragmentService(gridService);
            dataFragmentService.SetGridView(gridView);
            dataFragmentService.SetTileElementService(tileElementService);

            gridService.SetGridView(gridView);
            gridService.SetDataFragmentService(dataFragmentService);
            progressService = new ProgressTrackerService(dataFragmentService);
            ExposedProgressService = progressService;
            gridService.SetProgressService(progressService);

            codeShardTrackerService = new CodeShardTrackerService();
            tileElementService.SetCodeShardTracker(codeShardTrackerService);
            ExposedTileElementService = tileElementService;
            ExposedGridService = gridService;

            injectService = new InjectService();
            systemIntegrityService = new SystemIntegrityService();
            systemIntegrityService.SetGameOverController(gameOverController);
            chatLogService = new ChatLogService(chatLogView);
            dotService = new DamageOverTimeService((SystemIntegrityService)systemIntegrityService);
            

            // Now that all services are created, inject them into TileElementService
            payloadManager = UnityEngine.Object.FindFirstObjectByType<PayloadManager>();

            gridService.SetPayloadManager(payloadManager);
            inputController.SetPayloadManager(payloadManager);
            tileElementService.SetPayloadManager(payloadManager);
            dataFragmentService.SetPayloadManager(payloadManager);
            injectController.SetPayloadManager(payloadManager);
            extractController.SetPayloadManager(payloadManager);
            roundPopupManager.SetPayloadManager(payloadManager);
            injectService.SetPayloadManager(payloadManager);
            gridStateService.SetPayloadManager(payloadManager);

            tileElementService.SetInjectService(injectService);
            tileElementService.SetSystemIntegrityService(systemIntegrityService);
            tileElementService.SetVirusService(virusService);

            symbolToolService = new SymbolToolService(gridService, virusService);
            gridService.SetSymbolToolService(symbolToolService);
            injectService.SetSymbolToolService(symbolToolService);
            tileElementService.SetChatLogService(chatLogService);
            tileElementService.SetDataFragmentService(dataFragmentService);
            tileElementService.SetProgressTrackerService(progressService);

            inputController.Initialize(gridService, injectService, tileElementService, gridView, symbolToolService, chatLogService, payloadManager, dotService);
            inputController.SetVirusService(virusService);
            
            inputController.SetSystemIntegrityService(systemIntegrityService);

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
                tileElementService,
                symbolToolService,
                payloadManager,
                systemIntegrityService,
                roundPopupManager
            );

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
            gridView.RebindSymbolToolService(symbolToolService);
            tileElementService.ResizeGrid(gridService.GridWidth, gridService.GridHeight);
            gridView.RefreshGrid(gridService);

            injectController.Initialize(injectService, gridService, chatLogService);
            injectController.SetChatLogService(chatLogService);

            extractController.Initialize(
                gridService, 
                progressService, 
                dataFragmentService, 
                codeShardTrackerService, 
                tileElementService, 
                roundService, 
                roundPopupManager, 
                extractService,
                systemIntegrityService,
                roundManager
            );

            roundPopupManager.SetTileElementService(tileElementService);

            systemIntegrityService.SetGameOverController(gameOverController);
            csTrackerView.Initialize(codeShardTrackerService);

            gridService.SetChatLogService(chatLogService);
            gridService.OnGridUpdated += () =>
            {
                gridView.RefreshVirusLabels(); // 🆕 Update row/column label counts
            };

            roundService.onRoundReset += () =>
            {
                // Rebuild visual grid BEFORE refreshing it
                gridView.BuildGrid(gridService.GridWidth, gridService.GridHeight, (x, y, button) => inputController.HandleTileClick(x, y, button));
                gridView.RebindSymbolToolService(symbolToolService);

                int indicatorCount = roundManager.GetCurrentIndicatorCount();
                gridView.SetVisibleIndicators(indicatorCount, indicatorCount, gridService.GridHeight, gridService.GridWidth);
                gridView.ApplyIndicatorVisibility(); //apply to freshly built label objects

                gridView.RefreshGrid(gridService);
                progressTrackerView.Refresh();
                injectController.RefreshUI();
            };
            StartCoroutine(BeginIntroChatSequence());

            debugController = FindObjectOfType<DebugController>();
        }

        private void Start()
        {
            if (debugController != null)
            {
                debugController.Initialize(gridService, dataFragmentService, progressService);
            }
        }

        private IEnumerator BeginIntroChatSequence()
        {
            //chatLogService.SystemMessage("booting...", ChatMessageType.Info, ChatDisplayMode.Instant);
            //yield return new WaitForSeconds(0.2f);

            //chatLogService.SystemMessage("loading data fragments...", ChatMessageType.Info, ChatDisplayMode.Instant);
            yield return new WaitForSeconds(0.5f);

            chatLogService.Log("let's get to work. press <color=#E44E4E>TAB</color> to open the commander. press <color=#E44E4E>INJECT</color> to begin.", ChatMessageType.Info, ChatDisplayMode.Typewriter);
            yield return null;
        }
    }
}