using System.Collections.Generic;
using System;
using NewGameplay.Services;
using NewGameplay.Controllers;

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
        void AddTool(string toolName);
        string SelectedTool { get; }
        void SetPayloadManager(PayloadManager payloadManager);

    }
} 