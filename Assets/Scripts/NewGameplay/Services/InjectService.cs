using System.Collections.Generic;
using UnityEngine;
using NewGameplay.Interfaces;
using System;
using NewGameplay.Enums;

namespace NewGameplay.Services
{
    public class InjectService : IInjectService
    {
        private readonly List<string> currentTools = new List<string>();
        private int selectedToolIndex = -1;
        private readonly IPayloadService payloadService;
        private readonly ISymbolToolService symbolToolService;

        public event Action OnToolsUpdated;
        public event Action OnToolSelected;

        public string SelectedTool => selectedToolIndex >= 0 && selectedToolIndex < currentTools.Count ? currentTools[selectedToolIndex] : null;

        public InjectService(IPayloadService payloadService, ISymbolToolService symbolToolService)
        {
            this.payloadService = payloadService ?? throw new ArgumentNullException(nameof(payloadService));
            this.symbolToolService = symbolToolService ?? throw new ArgumentNullException(nameof(symbolToolService));
        }

        public void ResetForNewRound()
        {
            string fourthTool = currentTools.Count > 3 ? currentTools[3] : null;
            currentTools.Clear();

            currentTools.Add(ToolConstants.PURGE_TOOL);
            currentTools.Add(ToolConstants.FORK_TOOL);
            currentTools.Add(ToolConstants.PIVOT_TOOL);

            if (payloadService.IsPayloadActive(PayloadType.ToolkitExpansion))
            {
                string[] availableTools = new string[]
                {
                    ToolConstants.PURGE_TOOL,
                    ToolConstants.FORK_TOOL,
                    ToolConstants.PIVOT_TOOL
                };

                string randomTool = availableTools[UnityEngine.Random.Range(0, availableTools.Length)];
                currentTools.Add(randomTool);
            }

            OnToolsUpdated?.Invoke();
        }

        public List<string> GetCurrentTools()
        {
            return new List<string>(currentTools);
        }

        public void AddTool(string toolName)
        {
            if (string.IsNullOrEmpty(toolName))
                throw new ArgumentException("Tool name cannot be null or empty", nameof(toolName));

            if (currentTools.Count >= 4) return;

            currentTools.Add(toolName);
            OnToolsUpdated?.Invoke();
        }

        public void SetSelectedTool(int index)
        {
            if (index < 0 || index >= currentTools.Count) return;
            
            selectedToolIndex = index;
            symbolToolService.SetSelectedTool(currentTools[index]);
            OnToolSelected?.Invoke();
        }

        public void ClearSelectedTool()
        {
            selectedToolIndex = -1;
            symbolToolService.SetSelectedTool(null);
            OnToolSelected?.Invoke();
        }

        public void UseSelectedTool()
        {
            if (selectedToolIndex < 0 || selectedToolIndex >= currentTools.Count) return;
            // Tool usage is handled by SymbolToolService
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
            symbolToolService.SetSelectedTool(null);
            
            OnToolsUpdated?.Invoke();
            OnToolSelected?.Invoke();
        }

        public void ClearToolBank()
        {
            currentTools.Clear();
            selectedToolIndex = -1;
            symbolToolService.SetSelectedTool(null);
            
            OnToolsUpdated?.Invoke();
            OnToolSelected?.Invoke();
        }
    }
}