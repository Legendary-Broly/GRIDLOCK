using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using NewGameplay.Interfaces;
using NewGameplay.Services;
using System.Linq;
using TMPro;
using NewGameplay.Views;
using NewGameplay.ScriptableObjects;
using NewGameplay.Enums;
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
        private GridViewNew gridView;
        private IChatLogService chatLogService;
        private PayloadManager payloadManager;
        public void SetPayloadManager(PayloadManager manager) => payloadManager = manager;
        public void Initialize(IInjectService injectService, IGridService gridService, IChatLogService chatLogService)
        {
            this.injectService = injectService;
            this.gridService = gridService;
            this.chatLogService = chatLogService;

            injectService.OnToolsUpdated += RefreshUI;
            injectService.OnToolSelected += UpdateToolSelection;

            for (int i = 0; i < toolButtons.Count; i++)
            {
                int index = i; // Capture for lambda
                toolButtons[i].onClick.AddListener(() => OnToolButtonClicked(index));
            }
               if (injectButton != null)
                injectButton.onClick.AddListener(OnInject);

            RefreshUI();
        }
        public void SetChatLogService(IChatLogService chatLogService)
        {
            this.chatLogService = chatLogService;
        }
        private void OnInject()
        {
            if (injectService == null || (injectButton != null && !injectButton.interactable)) return;

            // (1) reset the symbol‐bank as you already do
            injectService.ResetForNewRound();

            RefreshUI();
            gridService.UnlockInteraction();            // allow clicks
            gridService.SetFirstRevealPermitted(true);  // allow the very first reveal
            
            // (2) pick a random empty/playable tile
            var positions = gridService.GetValidInitialRevealPositions();
            if (positions.Count > 0)
            {
                var pos = positions[UnityEngine.Random.Range(0, positions.Count)];
                // force a reveal (skips adjacency check)
                gridService.RevealTile(pos.x, pos.y, forceReveal: true);
                gridService.SetLastRevealedTile(pos);
                gridService.SetFirstRevealPermitted(false); // Reset after reveal to ensure next move must be adjacent
            }


            // (3) finally repaint the grid
            var gridView = FindFirstObjectByType<GridViewNew>();
            if (gridView != null)
                gridView.RefreshGrid(gridService);

            gridService.TriggerGridUpdate();

            SetInjectButtonInteractable(false); // Disable after use

            chatLogService?.LogRandomInjectLine();
        }


        private void OnToolButtonClicked(int index)
        {
            if (injectService == null) return;

            string tool = GetToolAt(index);
            if (string.IsNullOrEmpty(tool)) return;

            injectService.SetSelectedTool(index);
            UpdateToolSelection();

            // If the selected tool is Pivot, immediately activate it and refresh the grid
            if (tool == ToolConstants.PIVOT_TOOL)
            {
                var inputController = FindFirstObjectByType<GridInputController>();
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

            // Update all tool slots
            for (int i = 0; i < toolSlots.Count; i++)
            {
                bool hasTool = i < tools.Count;
                toolSlots[i].text = hasTool ? tools[i] : "";
                
                // Special handling for the fourth slot
                if (i == 3)
                {
                    bool toolkitExpansionActive = payloadManager != null && payloadManager.IsPayloadActive(PayloadType.ToolkitExpansion);
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
            // No color adjustment here; only interactable state is managed in code.
        }
        public void EnableNextToolSlot()
        {
            if (toolButtonRoot == null)
            {
                Debug.LogError("[ToolkitController] ToolButton root not assigned.");
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

            string[] availableTools = new string[]
            {
                ToolConstants.PURGE_TOOL,
                ToolConstants.FORK_TOOL,
                ToolConstants.PIVOT_TOOL
            };

            // Choose one at random
            string selectedTool = availableTools[UnityEngine.Random.Range(0, availableTools.Length)];

            var tools = injectService.GetCurrentTools();
            if (tools.Count >= 4)
            {

                return;
            }

            injectService.AddTool(selectedTool); // 🆕 (see next step)


            RefreshUI(); // updates button visuals and interactivity
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
            {
                injectButton.interactable = interactable;
            }
        }
    }
}
