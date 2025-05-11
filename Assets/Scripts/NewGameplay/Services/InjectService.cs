using System.Collections.Generic;
using UnityEngine;
using NewGameplay.Interfaces;
using System;

namespace NewGameplay.Services
{
    public class InjectService : IInjectService
    {
        private List<string> currentTools = new List<string>();
        private int selectedToolIndex = -1;
        private ISymbolToolService symbolToolService;

        public event Action OnToolsUpdated;
        public event Action OnToolSelected;

        public string SelectedTool => selectedToolIndex >= 0 && selectedToolIndex < currentTools.Count ? currentTools[selectedToolIndex] : null;

        public void SetSymbolToolService(ISymbolToolService service)
        {
            this.symbolToolService = service;
        }

        public void ResetForNewRound()
        {
            currentTools.Clear();
            currentTools.Add(ToolConstants.PURGE_TOOL);
            currentTools.Add(ToolConstants.FORK_TOOL);
            currentTools.Add(ToolConstants.PIVOT_TOOL);
            selectedToolIndex = -1;
            OnToolsUpdated?.Invoke();
        }

        public List<string> GetCurrentTools()
        {
            return new List<string>(currentTools);
        }

        public void SetSelectedTool(int index)
        {
            if (index < 0 || index >= currentTools.Count) return;
            
            selectedToolIndex = index;
            OnToolSelected?.Invoke();
        }

        public void ClearSelectedTool()
        {
            selectedToolIndex = -1;
            OnToolSelected?.Invoke();
        }

        public void UseSelectedTool()
        {
            if (selectedToolIndex < 0 || selectedToolIndex >= currentTools.Count) return;
            // No tool effect logic here. Only selection management.
        }

        public string GetSelectedTool()
        {
            return selectedToolIndex >= 0 && selectedToolIndex < currentTools.Count ? currentTools[selectedToolIndex] : null;
        }

        public void RemoveSelectedTool()
        {
            if (selectedToolIndex < 0 || selectedToolIndex >= currentTools.Count) return;
            currentTools.RemoveAt(selectedToolIndex);
            selectedToolIndex = -1;
            OnToolsUpdated?.Invoke();
            OnToolSelected?.Invoke();
        }

        public void ClearToolBank()
        {
            currentTools.Clear();
            selectedToolIndex = -1;
            OnToolsUpdated?.Invoke();
            OnToolSelected?.Invoke();
        }
    }
}