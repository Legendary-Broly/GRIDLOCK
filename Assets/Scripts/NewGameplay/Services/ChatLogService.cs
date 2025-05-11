using NewGameplay.Interfaces;
using NewGameplay.Views;
using NewGameplay.ScriptableObjects;
using NewGameplay.Enums;
using System.Collections.Generic;

namespace NewGameplay.Services
{
    public class ChatLogService : IChatLogService
    {
        private readonly ChatLogView _chatLogView;
        private string lastCodeShardLine;
        private string lastSystemIntegrityLine;
        private string lastToolRefreshLine;
        private string lastVirusLine;

        public ChatLogService(ChatLogView chatLogView)
        {
            _chatLogView = chatLogView;
        }

        public void Log(string message, ChatMessageType type = ChatMessageType.Info, ChatDisplayMode mode = ChatDisplayMode.Typewriter)
        {
            _chatLogView?.DisplayMessage($"<b>mgr_smile:</b> {message}", type, mode);
        }

        public void SystemMessage(string message, ChatMessageType type = ChatMessageType.Info, ChatDisplayMode mode = ChatDisplayMode.Typewriter)
        {
            _chatLogView?.DisplayMessage($"<b>[SYSTEM]:</b> {message}", type, mode);
        }
        private readonly List<string> injectMessages = new()
        {
            "scanning for safe injection points...",
            "targeting safe injection point...",
            "running re-injection sequence...",
            "yes, yes this is safe.",
            "breaching new injection port...",
        };
        private readonly List<string> codeShardLines = new()
        {
            "code shard recovered.",
            "that looks valuable.",
            "a few more of those and we'll rich.",
            "money in the bank."
        };

        private readonly List<string> systemIntegrityLines = new()
        {
            "system patch applied.",
            "integrity restored. barely.",
            "ah, that's better.",
            "better than duct tape."
        };

        private readonly List<string> toolRefreshLines = new()
        {
            "tools refilled. for free this time.",
            "you got your toys back.",
            "more tools. don't waste them.",
        };

        public void LogRandomInjectLine()
        {
            if (injectMessages.Count == 0) return;

            int index = UnityEngine.Random.Range(0, injectMessages.Count);
            string line = injectMessages[index];

            _chatLogView?.DisplayMessage($"<b>mgr_smile:</b> {line}", ChatMessageType.Info, ChatDisplayMode.Typewriter);
        }
        public void LogTileElementReveal(TileElementType type)
        {
            string line = type switch
            {
                TileElementType.CodeShard => GetRandomLine(codeShardLines, ref lastCodeShardLine),
                TileElementType.SystemIntegrityIncrease => GetRandomLine(systemIntegrityLines, ref lastSystemIntegrityLine),
                TileElementType.ToolRefresh => GetRandomLine(toolRefreshLines, ref lastToolRefreshLine),
                _ => null
            };

            if (!string.IsNullOrEmpty(line))
            {
                _chatLogView?.DisplayMessage($"<b>mgr_smile:</b> {line}", ChatMessageType.Info, ChatDisplayMode.Typewriter);
            }
        }

        private string GetRandomLine(List<string> pool, ref string lastLine)
        {
            if (pool.Count == 0) return "";

            string line;
            do
            {
                line = pool[UnityEngine.Random.Range(0, pool.Count)];
            }
            while (pool.Count > 1 && line == lastLine);

            lastLine = line;
            return line;
        }
        private readonly List<string> virusRevealLines = new()
        {
            ">>> VIRUS DETECTED <<<",
            "!!! SYSTEM BREACH !!!",
            ">>> WARNING: VIRUS <<<",
            "!!! THREAT LOCATED !!!"
        };
        public void LogVirusReveal()
        {
            string line = GetRandomLine(virusRevealLines, ref lastVirusLine);

            // Styled for emphasis: red, bold, larger
            string formatted = $"<size=24><color=#FF4C4C><b>{line}</b></color></size>";
            _chatLogView?.DisplayMessage(formatted, ChatMessageType.Virus, ChatDisplayMode.Instant);
        }
    }
}
