using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using NewGameplay.Interfaces;
using NewGameplay.Views;
using NewGameplay.ScriptableObjects;
using NewGameplay.Enums;
using NewGameplay.Services;

namespace NewGameplay.Controllers
{
    public class InjectController : MonoBehaviour
    {
        [SerializeField] private List<Button> toolButtons;
        [SerializeField] private List<TextMeshProUGUI> toolSlots;
        [SerializeField] private Button injectButton;
        [SerializeField] private Transform toolButtonRoot;

        private IInjectService injectService;
        private IGridService gridService;
        private IChatLogService chatLogService;
        private GridViewNew gridView;
        private GridInputController inputController;
        private PayloadManager payloadManager;

        public void Initialize(
            IInjectService injectService,
            IGridService gridService,
            IChatLogService chatLogService)
        {
            this.injectService = injectService;
            this.gridService = gridService;
            this.chatLogService = chatLogService;

            injectService.OnToolsUpdated += RefreshUI;
            injectService.OnToolSelected += UpdateToolSelection;

            for (int i = 0; i < toolButtons.Count; i++)
            {
                int index = i;
                toolButtons[i].onClick.AddListener(() => OnToolButtonClicked(index));
            }

            if (injectButton != null)
                injectButton.onClick.AddListener(OnInject);

            RefreshUI();
        }

        public void SetGridView(GridViewNew view) => gridView = view;
        public void SetInputController(GridInputController controller) => inputController = controller;
        public void SetPayloadManager(PayloadManager manager) => payloadManager = manager;
        public void SetChatLogService(IChatLogService chatLogService) => this.chatLogService = chatLogService;

        private void OnInject()
        {
            if (injectService == null || (injectButton != null && !injectButton.interactable)) return;

            injectService.ResetForNewRound();
            RefreshUI();
            gridService.UnlockInteraction();
            gridService.SetFirstRevealPermitted(true);

            var positions = gridService.GetValidInitialRevealPositions();
            if (positions.Count > 0)
            {
                var pos = positions[Random.Range(0, positions.Count)];
                gridService.RevealTile(pos.x, pos.y, true);
                gridService.SetLastRevealedTile(pos);
                gridService.SetFirstRevealPermitted(false);
            }

            gridView?.RenderGrid();
            gridService.TriggerGridUpdate();
            SetInjectButtonInteractable(false);
            chatLogService?.LogRandomInjectLine();
        }

        private void OnToolButtonClicked(int index)
        {
            if (injectService == null) return;

            string tool = GetToolAt(index);
            if (string.IsNullOrEmpty(tool)) return;

            injectService.SetSelectedTool(index);
            UpdateToolSelection();

            if (tool == ToolConstants.PIVOT_TOOL)
            {
                inputController?.ActivatePivotToolAndRefreshGrid();
            }
        }

        private string GetToolAt(int index)
        {
            if (injectService == null) return "?";
            var tools = injectService.GetCurrentTools();
            return index < tools.Count ? tools[index] : "?";
        }

        public void RefreshUI()
        {
            if (injectService == null) return;
            var tools = injectService.GetCurrentTools();

            for (int i = 0; i < toolSlots.Count; i++)
            {
                bool hasTool = i < tools.Count;
                toolSlots[i].text = hasTool ? tools[i] : "";

                if (i == 3)
                {
                    bool toolkitExpansionActive = payloadManager != null &&
                        payloadManager.IsPayloadActive(PayloadType.ToolkitExpansion);

                    toolButtons[i].gameObject.SetActive(toolkitExpansionActive);
                    toolButtons[i].interactable = toolkitExpansionActive && hasTool;
                }
                else
                {
                    toolButtons[i].interactable = hasTool;
                }
            }

            UpdateToolSelection();
        }

        private void UpdateToolSelection()
        {
            if (injectService == null) return;
            string selectedTool = injectService.SelectedTool;
            // Visual feedback handled via UI prefab configuration
        }

        public void EnableNextToolSlot()
        {
            if (toolButtonRoot == null)
            {
                Debug.LogError("[InjectController] ToolButton root not assigned.");
                return;
            }

            var allButtons = toolButtonRoot.GetComponentsInChildren<Transform>(true);
            foreach (var button in allButtons)
            {
                if (button.name.StartsWith("ToolButton") && !button.gameObject.activeSelf)
                {
                    button.gameObject.SetActive(true);
                    return;
                }
            }
        }

        public void AssignRandomToolToNextSlot()
        {
            if (injectService == null) return;

            string[] availableTools = {
                ToolConstants.PURGE_TOOL,
                ToolConstants.FORK_TOOL,
                ToolConstants.PIVOT_TOOL
            };

            string selectedTool = availableTools[Random.Range(0, availableTools.Length)];

            var tools = injectService.GetCurrentTools();
            if (tools.Count >= 4) return;

            injectService.AddTool(selectedTool);
            RefreshUI();
        }

        public void ClearToolSlots()
        {
            for (int i = 0; i < toolSlots.Count; i++)
            {
                toolSlots[i].text = "";
                toolButtons[i].interactable = false;
            }
        }

        public void SetInjectButtonInteractable(bool interactable)
        {
            if (injectButton != null)
                injectButton.interactable = interactable;
        }
    }
}
