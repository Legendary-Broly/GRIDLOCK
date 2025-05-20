using System.Collections.Generic;
using UnityEngine;
using NewGameplay.Interfaces;
using System;
using NewGameplay.Controllers;
using NewGameplay.Enums;

namespace NewGameplay.Services
{
    public class InjectService : IInjectService
    {
        private List<string> currentTools = new List<string>();
        private int selectedToolIndex = -1;
        private ISymbolToolService symbolToolService;
        private PayloadManager payloadManager;
        public event Action OnToolsUpdated;
        public event Action OnToolSelected;

        public string SelectedTool => selectedToolIndex >= 0 && selectedToolIndex < currentTools.Count ? currentTools[selectedToolIndex] : null;
        public void SetPayloadManager(PayloadManager payloadManager)
        {
            this.payloadManager = payloadManager;
        }

        public void SetSymbolToolService(ISymbolToolService service)
        {
            this.symbolToolService = service;
        }

        public void ResetForNewRound()
        {

            string fourthTool = currentTools.Count > 3 ? currentTools[3] : null;
            currentTools.Clear();

            currentTools.Add(ToolConstants.PURGE_TOOL);
            currentTools.Add(ToolConstants.FORK_TOOL);
            currentTools.Add(ToolConstants.PIVOT_TOOL);

            if (payloadManager != null && payloadManager.IsPayloadActive(PayloadType.ToolkitExpansion))
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

        private int GetActiveToolSlotCount()
        {
            // If we have 4 tools, all slots are active
            if (currentTools.Count >= 4)
                return 4;
            
            // Otherwise, we have the standard 3 slots
            return 3;
        }


        public List<string> GetCurrentTools()
        {
            return new List<string>(currentTools);
        }
        public void AddTool(string toolName)
        {
            if (currentTools.Count >= 4) return;

            currentTools.Add(toolName);
            OnToolsUpdated?.Invoke();
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