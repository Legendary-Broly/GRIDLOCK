using System.Collections.Generic;
using System;

namespace NewGameplay.Interfaces
{
    public interface IInjectService
    {
        event Action OnToolsUpdated;
        event Action OnToolSelected;

        string SelectedTool { get; }

        void ResetForNewRound();
        List<string> GetCurrentTools();
        void AddTool(string toolName);
        void SetSelectedTool(int index);
        void ClearSelectedTool();
        void UseSelectedTool();
        string GetSelectedTool();
        void RemoveSelectedTool();
        void ClearToolBank();
    }
} 