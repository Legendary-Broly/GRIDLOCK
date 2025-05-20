using System;
using UnityEngine;

namespace NewGameplay.Interfaces
{
    public static class ToolConstants
    {
        public const string PURGE_TOOL = "PURGE";
        public const string FORK_TOOL = "FORK";
        public const string PIVOT_TOOL = "PIVOT";
        public const string VIRUS_SYMBOL = "X";
    }

    public interface ISymbolToolService
    {
        event Action OnToolUsed;
        event Action OnPivotActivated;
        event Action OnPivotDeactivated;

        void UsePurgeTool(int x, int y);
        void UseForkTool(int x, int y);
        void UsePivotTool();
        void DeactivatePivot();
        bool IsPivotActive();
    }
} 