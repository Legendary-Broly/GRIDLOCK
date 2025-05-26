using UnityEngine;
using SplitGrid.Interfaces;
using NewGameplay.Interfaces;

namespace SplitGrid.Services
{
    public static class SplitGridServiceLocator
    {
        private static ISplitGridService splitGridService;
        private static ISplitTileElementService splitTileElementService;
        private static ISplitVirusService splitVirusService;
        private static ISplitProgressTrackerService splitProgressTrackerService;
        private static ISplitRoundService splitRoundService;
        private static ISplitRoundPopupManager splitRoundPopupManager;
        private static IPayloadManager payloadManager;
        private static IChatLogService chatLogService;

        public static void Initialize(
            ISplitGridService gridService,
            ISplitTileElementService tileElementService,
            ISplitVirusService virusService,
            ISplitProgressTrackerService progressTrackerService,
            ISplitRoundService roundService,
            ISplitRoundPopupManager roundPopupManager,
            IPayloadManager payloadManager,
            IChatLogService chatLogService)
        {
            SplitGridServiceLocator.splitGridService = gridService;
            SplitGridServiceLocator.splitTileElementService = tileElementService;
            SplitGridServiceLocator.splitVirusService = virusService;
            SplitGridServiceLocator.splitProgressTrackerService = progressTrackerService;
            SplitGridServiceLocator.splitRoundService = roundService;
            SplitGridServiceLocator.splitRoundPopupManager = roundPopupManager;
            SplitGridServiceLocator.payloadManager = payloadManager;
            SplitGridServiceLocator.chatLogService = chatLogService;
        }

        public static ISplitGridService GetSplitGridService() => splitGridService;
        public static ISplitTileElementService GetSplitTileElementService() => splitTileElementService;
        public static ISplitVirusService GetSplitVirusService() => splitVirusService;
        public static ISplitProgressTrackerService GetSplitProgressTrackerService() => splitProgressTrackerService;
        public static ISplitRoundService GetSplitRoundService() => splitRoundService;
        public static ISplitRoundPopupManager GetSplitRoundPopupManager() => splitRoundPopupManager;
        public static IPayloadManager GetPayloadManager() => payloadManager;
        public static IChatLogService GetChatLogService() => chatLogService;

        public static void Reset()
        {
            splitGridService = null;
            splitTileElementService = null;
            splitVirusService = null;
            splitProgressTrackerService = null;
            splitRoundService = null;
            splitRoundPopupManager = null;
            payloadManager = null;
            chatLogService = null;
        }
    }
} 