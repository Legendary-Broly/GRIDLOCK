using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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
        if (injectService is InjectService raw)
            return raw.CurrentSymbolAt(index);
        return "?";
    }
    public void ClearSymbolSlots()
    {
        for (int i = 0; i < symbolSlots.Count; i++)
        {
            symbolSlots[i].text = ""; // Clear the symbol text from UI
            symbolButtons[i].interactable = false; // Optionally disable buttons
        }
    }

}
