using System.Collections.Generic;
using System;

namespace NewGameplay.Interfaces
{
    public interface IInjectService
    {
        event Action OnToolsUpdated;
        event Action OnToolSelected;

        void SetSymbolToolService(ISymbolToolService service);
        void ResetForNewRound();
        List<string> GetCurrentTools();
        void SetSelectedTool(int index);
        void ClearSelectedTool();
        void UseSelectedTool();
        string GetSelectedTool();
        void RemoveSelectedTool();
        void ClearToolBank();

        string SelectedTool { get; }
    }
} 