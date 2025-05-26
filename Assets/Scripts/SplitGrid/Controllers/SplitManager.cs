using UnityEngine;
using SplitGrid.Views;
using SplitGrid.Controllers;
using SplitGrid.Services;
using SplitGrid.Interfaces;
using NewGameplay.Services;
using NewGameplay.Interfaces;
using NewGameplay.Models;
using NewGameplay.Controllers;
using NewGameplay.Views;
using NewGameplay.UI;

namespace SplitGrid
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
        [SerializeField] private PayloadManager payloadManager;
        [SerializeField] private SplitRoundService splitRoundService;
        [SerializeField] private SplitRoundPopupManager splitRoundPopupManager;

        public void Initialize()
        {
            // Initialize service locator with all required services
            SplitGridServiceLocator.Initialize(
                splitGridService,
                tileElementServiceA,
                virusServiceA,
                splitProgressTrackerService,
                splitRoundService,
                splitRoundPopupManager,
                payloadManager,
                chatLogService
            );

            // Initialize controllers with their required dependencies
            splitGridController.Initialize(
                splitGridService,
                tileElementServiceA,
                tileElementServiceB,
                symbolToolServiceA,
                symbolToolServiceB,
                virusServiceA,
                virusServiceB,
                chatLogService
            );

            splitInjectController.Initialize(
                splitGridService,
                symbolToolServiceA,
                symbolToolServiceB,
                chatLogService
            );

            splitExtractController.Initialize(
                splitGridService,
                splitProgressTrackerService,
                virusServiceA,
                virusServiceB,
                chatLogService,
                payloadManager
            );

            splitGridInputController.Initialize(splitGridController);

            // Initialize view with all required dependencies
            splitGridView.Initialize(
                splitGridService,
                tileElementServiceA,
                tileElementServiceB,
                virusServiceA,
                virusServiceB,
                symbolToolServiceA,
                symbolToolServiceB,
                fragmentServiceA,
                fragmentServiceB,
                splitGridInputController
            );
        }

        private void OnDestroy()
        {
            SplitGridServiceLocator.Reset();
        }
    }
}
