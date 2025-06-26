using UnityEngine;
using NewGameplay.SplitGrid.Interfaces;
using NewGameplay.SplitGrid.Services;
using NewGameplay.SplitGrid.Views;
using NewGameplay.SplitGrid.Data;
using NewGameplay.SplitGrid.Controllers;
using NewGameplay.Services;
using NewGameplay.Interfaces;
using NewGameplay.Models;
using NewGameplay.Controllers;
using NewGameplay.Views;
using NewGameplay.UI;
using NewGameplay.Enums;
using System;

namespace NewGameplay.SplitGrid
{
    public class SplitManager : MonoBehaviour
    {
        [Header("Controllers")]
        [SerializeField] private SplitGridController splitGridController;
        [SerializeField] private SplitInjectController splitInjectController;
        [SerializeField] private SplitExtractController splitExtractController;
        [SerializeField] private SplitGridInputController splitGridInputController;

        [Header("Views")]
        [SerializeField] private SplitGridView splitGridView;

        [Header("Services")]
        [SerializeField] private SplitGridService splitGridService;
        [SerializeField] private SplitTileElementService tileElementServiceA;
        [SerializeField] private SplitTileElementService tileElementServiceB;
        [SerializeField] private SplitVirusService virusServiceA;
        [SerializeField] private SplitVirusService virusServiceB;
        [SerializeField] private SymbolToolService symbolToolServiceA;
        [SerializeField] private SymbolToolService symbolToolServiceB;
        [SerializeField] private DataFragmentService fragmentServiceA;
        [SerializeField] private DataFragmentService fragmentServiceB;
        [SerializeField] private SplitProgressTrackerService splitProgressTrackerService;
        [SerializeField] private ChatLogService chatLogService;
        [SerializeField] private PayloadService payloadService;
        [SerializeField] private SplitRoundService splitRoundService;
        [SerializeField] private InjectService injectService;

        public void Initialize()
        {
            // Initialize service locator with all required services
            SplitGridServiceLocator.Initialize(
                splitGridService as ISplitGridService,
                tileElementServiceA as ISplitTileElementService,
                virusServiceA as ISplitVirusService,
                splitProgressTrackerService as ISplitProgressTrackerService,
                splitRoundService as ISplitRoundService,
                payloadService as IPayloadService,
                chatLogService as IChatLogService
            );

            // Initialize controllers with their required depedndencies
            splitGridController.Initialize(
                splitGridService as ISplitGridService,
                tileElementServiceA as ISplitTileElementService,
                tileElementServiceB as ISplitTileElementService,
                symbolToolServiceA as ISymbolToolService,
                symbolToolServiceB as ISymbolToolService,
                virusServiceA as ISplitVirusService,
                virusServiceB as ISplitVirusService,
                chatLogService as IChatLogService
            );

            splitInjectController.Initialize(
                splitGridService as ISplitGridService,
                injectService as IInjectService,
                symbolToolServiceA as ISymbolToolService,
                symbolToolServiceB as ISymbolToolService,
                chatLogService as IChatLogService
            );

            splitExtractController.Initialize(
                splitGridService as ISplitGridService,
                splitProgressTrackerService as ISplitProgressTrackerService,
                virusServiceA as ISplitVirusService,
                virusServiceB as ISplitVirusService,
                chatLogService as IChatLogService,
                payloadService as IPayloadService
            );

            splitGridInputController.Initialize(splitGridService as ISplitGridService);

            // Initialize view with all required dependencies
            splitGridView.Initialize(
                splitGridService as ISplitGridService,
                tileElementServiceA as ISplitTileElementService,
                tileElementServiceB as ISplitTileElementService,
                virusServiceA as ISplitVirusService,
                virusServiceB as ISplitVirusService,
                symbolToolServiceA as ISymbolToolService,
                symbolToolServiceB as ISymbolToolService,
                fragmentServiceA as IDataFragmentService,
                fragmentServiceB as IDataFragmentService,
                splitGridInputController as IGridInputController
            );
        }

        private void OnDestroy()
        {
            SplitGridServiceLocator.Reset();
        }
    }
}
