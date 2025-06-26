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
using NewGameplay.SplitGrid.Interfaces;
using NewGameplay.SplitGrid.Services;
using NewGameplay.SplitGrid.Views;
using NewGameplay.SplitGrid.Data;
using NewGameplay.SplitGrid.Controllers;

public class NewGameplayBootstrapper : MonoBehaviour
{
    // Exposed services as interfaces
    public IGridService ExposedGridService { get; private set; }
    public ITileElementService ExposedTileElementService { get; private set; }
    public IProgressTrackerService ExposedProgressService { get; private set; }
    public IDataFragmentService ExposedDataFragmentService { get; private set; }
    public ISystemIntegrityService ExposedSystemIntegrityService { get; private set; }

    // Private service instances
    private IGridStateService gridStateService;
    private IVirusService virusService;
    private IInjectService injectService;
    private ICodeShardTrackerService codeShardTrackerService;
    private IDataFragmentService dataFragmentService;
    private IGridService gridService;
    private ITileElementService tileElementService;
    private IProgressTrackerService progressService;
    private IRoundService roundService;
    private IExtractService extractService;
    private SymbolToolService symbolToolService;
    private ISystemIntegrityService systemIntegrityService;
    private IChatLogService chatLogService;
    private IDamageOverTimeService dotService;
    private IPayloadService payloadService;
    private PayloadManager payloadManager;

    // Serialized view references
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

    // Split grid components
    [SerializeField] private NewGameplay.SplitGrid.SplitManager splitManager;
    [SerializeField] private GameObject gridFrame;
    [SerializeField] private GameObject singleGridContainer;

    private void Awake()
    {
        InitializeServices();
        
        bool useSplitGrid = roundService?.CurrentRoundConfig?.useSplitGrid ?? false;
        
        if (useSplitGrid)
        {
            InitializeSplitGrid();
        }
        else
        {
            InitializeSingleGrid();
        }
    }

    private void InitializeSplitGrid()
    {
        // Disable single grid components
        if (gridFrame != null) gridFrame.SetActive(false);
        if (singleGridContainer != null) singleGridContainer.SetActive(false);
        if (gridView != null) gridView.gameObject.SetActive(false);
        if (inputController != null) inputController.gameObject.SetActive(false);
        if (injectController != null) injectController.gameObject.SetActive(false);
        if (extractController != null) extractController.gameObject.SetActive(false);
        if (progressTrackerView != null) progressTrackerView.gameObject.SetActive(false);
        if (systemIntegrityTrackerView != null) systemIntegrityTrackerView.gameObject.SetActive(false);
        if (csTrackerView != null) csTrackerView.gameObject.SetActive(false);

        // Initialize split grid system
        splitManager.Initialize();
    }

    private void InitializeSingleGrid()
    {
        InitializeControllers();
        InitializeViews();
        WireUpEventHandlers();
        StartGame();
    }

    private void InitializeServices()
    {
        // Core services
        gridStateService = new GridStateService();
        gridService = new GridService(gridStateService, virusService, chatLogService);
        virusService = new VirusService(gridService, gridStateService);
        chatLogService = new ChatLogService(chatLogView);
        systemIntegrityService = new SystemIntegrityService();
        dotService = new DamageOverTimeService((SystemIntegrityService)systemIntegrityService);
        payloadService = new PayloadService();

        // Load configurations
        var tileElementConfigs = Resources.LoadAll<TileElementSO>("TileElements").ToList();

        // Initialize dependent services
        tileElementService = new TileElementService(7, 7, tileElementConfigs);
        dataFragmentService = new DataFragmentService(gridService);
        progressService = new ProgressTrackerService(dataFragmentService);
        codeShardTrackerService = new CodeShardTrackerService();
        symbolToolService = new SymbolToolService(gridService, virusService);
        injectService = new InjectService(payloadService, symbolToolService);
        roundService = new RoundService(gridStateService, gridService, progressService, injectService, dataFragmentService, virusService, tileElementService, roundConfigDatabase);
        extractService = new ExtractService(gridService, dataFragmentService);

        // Wire up service dependencies
        WireUpServiceDependencies();
    }

    private void WireUpServiceDependencies()
    {
        // GridService dependencies
        gridService.SetTileElementService(tileElementService);
        gridService.SetProgressService(progressService);
        gridService.SetDataFragmentService(dataFragmentService);
        gridService.SetSymbolToolService(symbolToolService);
        gridService.SetChatLogService(chatLogService);
        gridService.SetPayloadService(payloadService);

        // TileElementService dependencies
        tileElementService.SetGridService(gridService);
        tileElementService.SetCodeShardTracker(codeShardTrackerService);
        tileElementService.SetInjectService(injectService);
        tileElementService.SetSystemIntegrityService(systemIntegrityService);
        tileElementService.SetVirusService(virusService);
        tileElementService.SetChatLogService(chatLogService);
        tileElementService.SetDataFragmentService(dataFragmentService);
        tileElementService.SetProgressTrackerService(progressService);

        // Other service dependencies
        dataFragmentService.SetTileElementService(tileElementService);
        dataFragmentService.SetPayloadService(payloadService);
        systemIntegrityService.SetGameOverController(gameOverController);
        gridStateService.SetPayloadService(payloadService);

        // Expose services
        ExposedGridService = gridService;
        ExposedTileElementService = tileElementService;
        ExposedProgressService = progressService;
        ExposedDataFragmentService = dataFragmentService;
        ExposedSystemIntegrityService = systemIntegrityService;
    }

    private void InitializeControllers()
    {
        inputController.Initialize(
            gridService,
            injectService,
            tileElementService,
            gridView,
            symbolToolService,
            chatLogService,
            payloadService,
            dotService
        );

        injectController.Initialize(injectService, gridService, chatLogService);
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
    }

    private void InitializeViews()
    {
        gridView.BuildGrid(
            gridService.GridWidth,
            gridService.GridHeight,
            (col, h) => virusService.CountVirusesInColumn(col, h),
            (row, w) => virusService.CountVirusesInRow(row, w),
            (x, y, button) => inputController.HandleTileClick(x, y, button),
            (x, y, slot) =>
            {
                slot.Initialize(x, y, gridService, virusService, tileElementService, symbolToolService, 
                    (tx, ty, btn) => inputController.HandleTileClick(tx, ty, btn));
                slot.SetDataFragmentService(dataFragmentService);
            }
        );

        progressTrackerView.Initialize(progressService, gridService);
        systemIntegrityTrackerView.Initialize(systemIntegrityService);
        csTrackerView.Initialize(codeShardTrackerService);
    }

    private void WireUpEventHandlers()
    {
        gridService.OnGridUpdated += () =>
        {
            gridView.RefreshVirusLabels(
                (col, h) => virusService.CountVirusesInColumn(col, h),
                (row, w) => virusService.CountVirusesInRow(row, w)
            );
        };

        roundService.onRoundReset += () =>
        {
            gridView.BuildGrid(
                gridService.GridWidth,
                gridService.GridHeight,
                (col, h) => virusService.CountVirusesInColumn(col, h),
                (row, w) => virusService.CountVirusesInRow(row, w),
                (x, y, button) => inputController.HandleTileClick(x, y, button),
                (x, y, slot) =>
                {
                    slot.Initialize(x, y, gridService, virusService, tileElementService, symbolToolService, 
                        (tx, ty, btn) => inputController.HandleTileClick(tx, ty, btn));
                    slot.SetDataFragmentService(dataFragmentService);
                }
            );

            int indicatorCount = roundManager.GetCurrentIndicatorCount();
            gridView.SetVisibleIndicators(indicatorCount, indicatorCount, gridService.GridHeight, gridService.GridWidth);
            gridView.ApplyIndicatorVisibility();
            gridView.RenderGrid();
            progressTrackerView.Refresh();
            injectController.RefreshUI();
        };
    }

    private void StartGame()
    {
        roundService.ResetRound();
        tileElementService.ResizeGrid(gridService.GridWidth, gridService.GridHeight);
        roundManager.StartFirstRound();
        StartCoroutine(BeginIntroChatSequence());
    }

    private IEnumerator BeginIntroChatSequence()
    {
        // Implementation of intro sequence
        yield break;
    }
}