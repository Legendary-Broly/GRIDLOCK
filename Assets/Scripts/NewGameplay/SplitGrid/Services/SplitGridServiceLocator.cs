using UnityEngine;
using NewGameplay.SplitGrid.Interfaces;
using NewGameplay.Interfaces;

namespace NewGameplay.SplitGrid.Services
{
    public static class SplitGridServiceLocator
    {
        private static ISplitGridService splitGridService;
        private static ISplitTileElementService tileElementService;
        private static ISplitVirusService virusService;
        private static ISplitProgressTrackerService progressTrackerService;
        private static ISplitRoundService roundService;
        private static IPayloadService payloadService;
        private static IChatLogService chatLogService;

        public static void Initialize(
            ISplitGridService splitGridService,
            ISplitTileElementService tileElementService,
            ISplitVirusService virusService,
            ISplitProgressTrackerService progressTrackerService,
            ISplitRoundService roundService,
            IPayloadService payloadService,
            IChatLogService chatLogService)
        {
            SplitGridServiceLocator.splitGridService = splitGridService;
            SplitGridServiceLocator.tileElementService = tileElementService;
            SplitGridServiceLocator.virusService = virusService;
            SplitGridServiceLocator.progressTrackerService = progressTrackerService;
            SplitGridServiceLocator.roundService = roundService;
            SplitGridServiceLocator.payloadService = payloadService;
            SplitGridServiceLocator.chatLogService = chatLogService;
        }

        public static void Reset()
        {
            splitGridService = null;
            tileElementService = null;
            virusService = null;
            progressTrackerService = null;
            roundService = null;
            payloadService = null;
            chatLogService = null;
        }

        public static ISplitGridService GetSplitGridService() => splitGridService;
        public static ISplitTileElementService GetTileElementService() => tileElementService;
        public static ISplitVirusService GetVirusService() => virusService;
        public static ISplitProgressTrackerService GetProgressTrackerService() => progressTrackerService;
        public static ISplitRoundService GetRoundService() => roundService;
        public static IPayloadService GetPayloadService() => payloadService;
        public static IChatLogService GetChatLogService() => chatLogService;
    }
} 