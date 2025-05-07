using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using NewGameplay.Interfaces;
using NewGameplay.Utility;
using NewGameplay.Views;
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
            if (injectService == null || (injectButton != null && !injectButton.interactable)) return;

            // (1) reset the symbol‐bank as you already do
            injectService.SetFixedSymbols(new List<string>
            {
                "∆:/run_PURGE.exe",
                "Ψ:/run_FORK.exe",
                "Σ:/run_REPAIR.exe"
            });
            RefreshUI();
            gridService.UnlockInteraction();            // allow clicks
            gridService.SetFirstRevealPermitted(true);  // allow the very first reveal
            
            // (2) pick a random empty/playable tile
            var positions = gridService.GetAllEmptyTilePositions();
            if (positions.Count > 0)
            {
                var pos = positions[UnityEngine.Random.Range(0, positions.Count)];
                // force a reveal (skips adjacency check)
                gridService.RevealTile(pos.x, pos.y, forceReveal: true);
                gridService.SetLastRevealedTile(pos); // ⬅ custom setter we'll add
            }

            // (3) finally repaint the grid
            var gridView = FindFirstObjectByType<GridViewNew>();
            if (gridView != null)
                gridView.RefreshGrid(gridService);

            gridService.TriggerGridUpdate();

            SetInjectButtonInteractable(false); // Disable after use
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

            injectService.SetSelectedSymbol(index);

            if (symbol == "∆:/run_PURGE.exe")
            {
                // PURGE is selected for targeting
            }
            else
            {
                var bootstrapper = FindFirstObjectByType<NewGameplayBootstrapper>();
                var grid = bootstrapper?.ExposedGridService;
                var tileElements = bootstrapper?.ExposedTileElementService;

                if (grid != null && tileElements != null)
                {
                    Debug.Log($"[InjectController] Triggering effect for symbol {symbol} immediately (no placement)");
                    SymbolEffectProcessor.ApplySymbolEffectAtPlacement(symbol, -1, -1, grid, tileElements);
                }

                injectService.ClearSelectedSymbol();
                RefreshUI();
            }

            // Visual feedback (PURGE highlight)
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
            if (injectService == null) return "?";
            var symbols = injectService.GetCurrentSymbols();
            return index < symbols.Count ? symbols[index] : "?";
        }

        public void ClearSymbolSlots()
        {
            for (int i = 0; i < symbolSlots.Count; i++)
            {
                symbolSlots[i].text = "";
                symbolButtons[i].interactable = false;
            }
        }

        public void SetInjectButtonInteractable(bool interactable)
        {
            if (injectButton != null)
                injectButton.interactable = interactable;
        }
    }
}
