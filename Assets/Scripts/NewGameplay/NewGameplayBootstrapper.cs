using UnityEngine;
using NewGameplay.Interfaces;
using NewGameplay.Services;
using NewGameplay.Controllers;
using NewGameplay.UI;
using System.Linq;
using NewGameplay.Enums;
using NewGameplay.Views;
using NewGameplay.Strategies;
using NewGameplay.ScriptableObjects;
using System.Collections;
using NewGameplay.Data;

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
        private IGridService gridServiceA;
        private IGridService gridServiceB;
        private TileElementService tileElementService;
        private IProgressTrackerService progressService;
        private RoundService roundService;
        private ExtractService extractService;
        private SymbolToolService symbolToolService;
        private ISystemIntegrityService systemIntegrityService;
        private IChatLogService chatLogService;
        private DebugController debugController;
        private IDamageOverTimeService dotService;


        [Header("Standard Grid Services")]
        [SerializeField] private GridInputController inputController;
        [SerializeField] private GridViewNew gridView;
        [SerializeField] private InjectController injectController;
        [SerializeField] private ExtractController extractController;
        [SerializeField] private ProgressTrackerView progressTrackerView;
        [SerializeField] private SystemIntegrityTrackerView systemIntegrityTrackerView;
        [SerializeField] private RoundPopupManager roundPopupManager;
        [SerializeField] private CSTrackerView csTrackerView;
        [SerializeField] private RoundManager roundManager;
        [SerializeField] private RoundConfigDatabase roundConfigDatabase;
        [SerializeField] private GameOverController gameOverController;
        [SerializeField] private ChatLogView chatLogView;
        [SerializeField] private PayloadManager payloadManager;

        [Header("Split Grid Services")]
        [SerializeField] private GridInputController inputControllerA;
        [SerializeField] private GridInputController inputControllerB;
        [SerializeField] private GridViewNew gridViewA;
        [SerializeField] private GridViewNew gridViewB;
        [SerializeField] private SplitGridController splitGridController;
        [SerializeField] private SplitGridInputController splitGridInputController;
        [SerializeField] private SplitGridView splitGridView;


        
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
            gridServiceA = new GridService(gridStateService, virusService, chatLogService);
            gridServiceB = new GridService(gridStateService, virusService, chatLogService);
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
                roundPopupManager,
                splitGridController,
                splitGridView
            );

            gridView.Initialize(
                gridService,
                virusService,
                tileElementService,
                inputController,
                (x, y, button) => inputController.HandleTileClick(x, y, button),
                symbolToolService
            );
            
            gridView.SetDataFragmentService(dataFragmentService);

            
            roundService.ResetRound();
            tileElementService.ResizeGrid(gridService.GridWidth, gridService.GridHeight);

            roundManager.StartFirstRound();

            progressTrackerView.Initialize(progressService, gridService);
            systemIntegrityTrackerView.Initialize(systemIntegrityService);
            gridView.BuildGrid(gridService.GridWidth, gridService.GridHeight, (x, y, button) => inputController.HandleTileClick(x, y, button));
            gridView.RebindSymbolToolService(symbolToolService);
            tileElementService.ResizeGrid(gridService.GridWidth, gridService.GridHeight);
            gridView.RefreshGrid(gridService);

            injectController.Initialize(
                injectService,
                gridService,
                chatLogService,
                roundService,
                gridServiceA,
                gridServiceB,
                splitGridController,
                gridViewA,
                gridViewB
            );

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

            // === SPLIT GRID SETUP ===
            var gridStateA = new GridStateService();
            var virusA = new VirusService(gridStateA);
            var tileElementsA = new TileElementService(7, 7, tileElementConfigs);
            var gridA = new GridService(gridStateA, virusA, chatLogService);
            tileElementsA.SetGridService(gridA);
            gridA.SetTileElementService(tileElementsA);
            virusA.SetGridService(gridA);

            var fragmentA = new DataFragmentService(gridA);
            fragmentA.SetGridView(gridViewA);
            fragmentA.SetTileElementService(tileElementsA);
            gridA.SetDataFragmentService(fragmentA);
            var progressA = new ProgressTrackerService(fragmentA);
            gridA.SetProgressService(progressA);
            tileElementsA.SetDataFragmentService(fragmentA);
            tileElementsA.SetProgressTrackerService(progressA);

            // B Grid
            var gridStateB = new GridStateService();
            var virusB = new VirusService(gridStateB);
            var tileElementsB = new TileElementService(7, 7, tileElementConfigs);
            var gridB = new GridService(gridStateB, virusB, chatLogService);
            tileElementsB.SetGridService(gridB);
            gridB.SetTileElementService(tileElementsB);
            virusB.SetGridService(gridB);

            var fragmentB = new DataFragmentService(gridB);
            fragmentB.SetGridView(gridViewB);
            fragmentB.SetTileElementService(tileElementsB);
            gridB.SetDataFragmentService(fragmentB);
            var progressB = new ProgressTrackerService(fragmentB);
            gridB.SetProgressService(progressB);
            tileElementsB.SetDataFragmentService(fragmentB);
            tileElementsB.SetProgressTrackerService(progressB);

            // Shared dependencies
            tileElementsA.SetSystemIntegrityService(systemIntegrityService);
            tileElementsB.SetSystemIntegrityService(systemIntegrityService);
            tileElementsA.SetInjectService(injectService);
            tileElementsB.SetInjectService(injectService);
            tileElementsA.SetPayloadManager(payloadManager);
            tileElementsB.SetPayloadManager(payloadManager);
            tileElementsA.SetChatLogService(chatLogService);
            tileElementsB.SetChatLogService(chatLogService);
            tileElementsA.SetVirusService(virusA);
            tileElementsB.SetVirusService(virusB);

            fragmentA.SetPayloadManager(payloadManager);
            fragmentB.SetPayloadManager(payloadManager);

            gridStateA.SetPayloadManager(payloadManager);
            gridStateB.SetPayloadManager(payloadManager);

            // Tracker service
            var splitFlagManager = new SplitFlagManager();
            var splitService = new SplitGridServiceLocator(gridA, gridB, virusA, virusB, tileElementsA, tileElementsB, fragmentA, fragmentB, payloadManager);

            splitGridController.Initialize(
                gridA, gridB,
                virusA, virusB,
                splitService, splitFlagManager,
                tileElementsA, tileElementsB,
                fragmentA, fragmentB,
                inputControllerA, inputControllerB,
                gridViewA, gridViewB,
                splitGridInputController,
                progressService,
                progressTrackerView,
                injectController
            );

            splitGridInputController.Initialize(inputControllerA, inputControllerB, splitGridController);

            // Inject remaining dependencies into the views/controllers
            gridViewA.Initialize(gridA, virusA, tileElementsA, inputControllerA, (x, y, btn) => splitGridInputController.HandleTileClick(x, y, GridID.A, btn));
            gridViewB.Initialize(gridB, virusB, tileElementsB, inputControllerB, (x, y, btn) => splitGridInputController.HandleTileClick(x, y, GridID.B, btn));
            gridViewA.SetDataFragmentService(fragmentA);
            gridViewB.SetDataFragmentService(fragmentB);

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