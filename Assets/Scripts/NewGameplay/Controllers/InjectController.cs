using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using NewGameplay.Interfaces;
using NewGameplay.Services;

namespace NewGameplay.Controllers
{
    public class InjectController : MonoBehaviour
    {
        [SerializeField] private List<Button> symbolButtons;
        [SerializeField] private List<TextMeshProUGUI> symbolSlots;
        [SerializeField] private Button injectButton;

        private IInjectService injectService;
        private IGridService gridService;

        public void Initialize(IInjectService service, IGridService grid)
        {
            injectService = service;
            gridService = grid;

            for (int i = 0; i < symbolButtons.Count; i++)
            {
                int index = i;
                symbolButtons[i].onClick.AddListener(() => OnSymbolClicked(index));
            }

            injectButton.onClick.AddListener(OnInject);
            RefreshUI();
        }

        private void OnInject()
        {
            injectService.InjectSymbols();
            RefreshUI();

            // Get the entropy service from the bootstrapper
            var bootstrapper = FindFirstObjectByType<NewGameplayBootstrapper>();
            if (bootstrapper != null)
            {
                var entropyService = bootstrapper.ExposedEntropyService;
                if (entropyService != null)
                {
                    // Base entropy increase of 5%
                    entropyService.Increase(5);

                    // Count viruses and add 1% per virus
                    int virusCount = 0;
                    for (int y = 0; y < gridService.GridHeight; y++)
                    {
                        for (int x = 0; x < gridService.GridWidth; x++)
                        {
                            if (gridService.GetSymbolAt(x, y) == "X")
                            {
                                virusCount++;
                            }
                        }
                    }
                    if (virusCount > 0)
                    {
                        entropyService.Increase(virusCount);
                        Debug.Log($"[Inject] Added {virusCount}% entropy from {virusCount} viruses");
                    }

                    // Refresh the entropy view
                    var entropyView = FindFirstObjectByType<EntropyTrackerView>();
                    if (entropyView != null)
                    {
                        entropyView.Refresh();
                    }
                }
            }

            gridService.SpreadVirus();
            FindFirstObjectByType<GridView>().RefreshGrid(gridService);
        }

        public void RefreshUI()
        {
            for (int i = 0; i < symbolSlots.Count; i++)
            {
                string symbol = GetSymbolAt(i);
                symbolSlots[i].text = symbol;
                symbolButtons[i].interactable = !string.IsNullOrEmpty(symbol);
            }
        }

        private void OnSymbolClicked(int index)
        {
            injectService.SelectSymbol(index);

            for (int i = 0; i < symbolButtons.Count; i++)
            {
                var image = symbolButtons[i].targetGraphic;
                if (image != null)
                {
                    var cb = symbolButtons[i].colors;
                    image.color = (i == index) ? cb.selectedColor : cb.normalColor;
                }
            }
        }

        private string GetSymbolAt(int index)
        {
            if (injectService is WeightedInjectService raw)
                return raw.GetCurrentSymbols()[index];
            return "?";
        }

        public void ClearSymbolSlots()
        {
            // Clear the UI elements
            for (int i = 0; i < symbolSlots.Count; i++)
            {
                symbolSlots[i].text = "";
                symbolButtons[i].interactable = false;
            }
        }
    }
}
