using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using NewGameplay.Interfaces;
using NewGameplay.Services;
using NewGameplay.Utility;

namespace NewGameplay.Controllers
{
    public class InjectController : MonoBehaviour
    {
        [SerializeField] private List<Button> symbolButtons;
        [SerializeField] private List<TextMeshProUGUI> symbolSlots;
        [SerializeField] private Button injectButton;

        private IWeightedInjectService injectService;

        private IGridService gridService;
        private IEntropyService entropyService;

        public void Initialize(IWeightedInjectService service, IGridService grid)
        {
            injectService = service;
            gridService = grid;

            var bootstrapper = FindFirstObjectByType<NewGameplayBootstrapper>();
            if (bootstrapper != null)
            {
                entropyService = bootstrapper.ExposedEntropyService;
            }

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
            if (injectService == null) return;

            injectService.InjectSymbols();
            RefreshUI();

            if (entropyService != null)
            {
                entropyService.Increase(10); // Base 5% entropy increase per inject
            }
            else
            {
                Debug.LogWarning("[InjectController] EntropyService not found when injecting symbols.");
            }

            var gridView = FindFirstObjectByType<GridViewNew>();
            if (gridView != null)
            {
                gridView.RefreshGrid(gridService);
            }
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
            string symbol = GetSymbolAt(index);
            if (string.IsNullOrEmpty(symbol)) return;

            // ∆ = Purge — keep existing placement logic
            if (symbol == "∆:/run_PURGE.exe")
            {
                injectService.SelectSymbol(index);
            }
            else
            {
                // Always select the symbol before using it
                injectService.SelectSymbol(index);
                // Trigger effect immediately (no grid placement)
                var bootstrapper = FindFirstObjectByType<NewGameplayBootstrapper>();
                var grid = bootstrapper?.ExposedGridService;
                var entropy = bootstrapper?.ExposedEntropyService;
                var tileElements = bootstrapper?.ExposedTileElementService;

                if (grid != null && entropy != null && tileElements != null)
                {
                    Debug.Log($"[InjectController] Triggering effect for symbol {symbol} immediately (no placement)");
                    SymbolEffectProcessor.ApplySymbolEffectAtPlacement(symbol, -1, -1, grid, entropy, tileElements);
                }

                // Clear used symbol and refresh UI
                injectService.ClearSelectedSymbol();
                RefreshUI();
            }

            // Update visual selection (for ∆ only)
            for (int i = 0; i < symbolButtons.Count; i++)
            {
                var image = symbolButtons[i].targetGraphic;
                if (image != null)
                {
                    var cb = symbolButtons[i].colors;
                    image.color = (i == index && symbol == "∆:/run_PURGE.exe") ? cb.selectedColor : cb.normalColor;
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
            for (int i = 0; i < symbolSlots.Count; i++)
            {
                symbolSlots[i].text = "";
                symbolButtons[i].interactable = false;
            }
        }
    }
}
