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
        private string lastInjectLine;
        private string lastCorrectFlagLine;
        private string lastIncorrectFlagLine;
        private string lastDataFragmentLine;
        public void LogDataFragmentReveal()
        {
            string line = GetRandomLine(dataFragmentLines, ref lastDataFragmentLine);
            Log(line, ChatMessageType.Fragment, ChatDisplayMode.Typewriter);
        }
        public ChatLogService(ChatLogView chatLogView)
        {
            _chatLogView = chatLogView;
        }
        public void Log(string message, ChatMessageType type = ChatMessageType.Info, ChatDisplayMode mode = ChatDisplayMode.Typewriter)
        {
            string formatted = $"<b><color=#E44E4E>mgr_smile</color>:</b>\n{message}";
            _chatLogView?.DisplayMessage(formatted, type, mode);
        }
        public void SystemMessage(string message, ChatMessageType type = ChatMessageType.Info)
        {
            string formatted = $"<b>[SYSTEM]:</b> {message}";
            _chatLogView?.DisplayMessage(formatted, type, ChatDisplayMode.Typewriter);
        }
        public void LogVirusReveal()
        {
            string line = GetRandomLine(virusRevealLines, ref lastVirusLine);
            string formatted = $"<size=16><color=#FF4C4C><b>{line}</b></color></size>";
            _chatLogView?.DisplayMessage(formatted, ChatMessageType.Virus, ChatDisplayMode.Instant, skipTyping: true);
        }
        public void LogRandomInjectLine()
        {
            if (injectMessages.Count == 0) return;

            string line = GetRandomLine(injectMessages, ref lastInjectLine);
            Log(line, ChatMessageType.Info, ChatDisplayMode.Typewriter);
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
                Log(line, ChatMessageType.Info, ChatDisplayMode.Typewriter);
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
        public void LogCorrectFlag()
        {
            string line = GetRandomLine(correctFlagLines, ref lastCorrectFlagLine);
            Log(line, ChatMessageType.Info, ChatDisplayMode.Typewriter);
        }
        public void LogIncorrectFlag()
        {
            string line = GetRandomLine(incorrectFlagLines, ref lastIncorrectFlagLine);
            Log(line, ChatMessageType.Info, ChatDisplayMode.Typewriter);
        }
        private readonly List<string> virusRevealLines = new()
        {
            ">>> VIRUS DETECTED <<<",
            "!!! SYSTEM BREACH !!!",
            ">>> WARNING: VIRUS <<<",
            "!!! THREAT LOCATED !!!",
            ">>> SYSTEM FAILING <<<",
        };
        private readonly List<string> correctFlagLines = new()
        {
            "don't reveal that.",
            "good instincts.",
            "that's a virus.",
            "not bad.",
            "virus flagged.",
            "good eye.",
            "you saw that too, huh?",
            "marked and dangerous.",
            "i’d avoid that.",
            "yes. that one’s ugly.",
            "your instincts are improving.",
            "almost like you know what you’re doing.",
            "spicy little hazard, isn’t it?",
            "you’re learning.",
            "flag confirmed. menace contained."
        };
        private readonly List<string> incorrectFlagLines = new()
        {
            "that one's safe.",
            "no viruses there.",
            "that's not a virus.",
            "seems safe.",
            "looks safe to me.",
            "false alarm.",
            "clean tile. for now.",
            "you’re jumping at ghosts.",
            "no threat detected.",
            "flag wasted. oh well.",
            "too cautious? or just paranoid?",
            "you’ll regret wasting those.",
            "that one’s harmless. mostly.",
            "see? told you to trust me.",
            "you don’t have to fear everything, you know."
        };
        private readonly List<string> injectMessages = new()
        {
            "scanning for safe injection points...",
            "targeting safe injection point...",
            "running re-injection sequence...",
            "yes, yes this is safe.",
            "breaching new injection port...",
            "opening new surface layer...",
            "locating stable entry vector...",
            "poke it again. harder this time.",
            "this won’t backfire. probably.",
            "stand by. injection en route...",
            "compiling breach algorithm...",
            "threading the needle...",
            "spooling another access line...",
            "let’s crack this grid open.",
            "found a soft spot."
        };
        private readonly List<string> codeShardLines = new()
        {
            "code shard recovered.",
            "that looks valuable.",
            "a few more of those and we'll rich.",
            "money in the bank.",
            "oh, that’s a good one.",
            "fragment secured. very shiny.",
            "keep finding those. trust me.",
            "another piece of the puzzle.",
            "currency. in raw form.",
            "good. we’ll spend that later.",
            "how many do you need? all of them.",
            "you’re building something. don’t forget.",
            "code is its own kind of weapon.",
            "hoarding these feels right, doesn’t it?"
        };
        private readonly List<string> systemIntegrityLines = new()
        {
            "system patch applied.",
            "integrity restored. barely.",
            "ah, that's better.",
            "better than duct tape.",
            "bandaged. for now.",
            "we’re holding together with string and spite.",
            "system’s still twitching. that counts.",
            "partial restoration complete. breathe.",
            "that should stop the bleeding.",
            "better. not safe. but better.",
            "stabilizing... there.",
            "don’t celebrate yet.",
            "you earned that patch. probably.",
            "one less alarm bell screaming in the dark."
        };
        private readonly List<string> toolRefreshLines = new()
        {
            "tools refilled. for free this time.",
            "you got your toys back.",
            "more tools. don't waste them.",
            "tools refreshed.",
            "lucky you.",
            "fresh ammo in the chamber.",
            "gears are greased again.",
            "you’ll burn through those too.",
            "use them wisely. or don’t.",
            "full toolkit. just like old times.",
            "rearmed. reloaded. still doomed.",
            "how generous of the system.",
            "no excuses now.",
            "hope you don’t waste this gift.",
            "tools online. act accordingly."
        };
        private readonly List<string> dataFragmentLines = new()
        {
            "there it is.",
            "got one.",
            "that’s the one.",
            "fragment recovered. lovely.",
            "it’s not hiding anymore.",
            "you're getting good at this.",
            "i was starting to worry.",
            "extraction target acquired.",
            "juicy bit of data, isn’t it?",
            "don’t drop it this time.",
            "you see that? that’s progress.",
            "now we’re getting somewhere.",
            "good. now get the rest.",
            "one of many.",
            "see? the system <b>wants</b> you to win.",
            "it was waiting for you.",
            "now that’s a find.",
            "you earned that. i guess.",
            "what a lovely thing to unearth.",
            "it’s real. i promise.",
            "feels good, doesn’t it?",
            "you found it.",
            "yes. this one’s important.",
            "don’t show this to anyone.",
        };
    }
}
