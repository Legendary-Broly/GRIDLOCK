using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardSlotController : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text label;
    [SerializeField] private Image selectionOutline; // Optional, assign in prefab

    private SymbolCard currentCard;
    private GameplayUIController uiController;

    public SymbolCard Card => currentCard;

    public void Initialize(SymbolCard card, GameplayUIController ui)
    {
        currentCard = card;
        uiController = ui;

        icon.sprite = card.Data.symbolSprite;
        label.text = card.Data.symbolName;
        SetSelectedVisual(false);
    }

    public void SetSelectedVisual(bool isSelected)
    {
        if (selectionOutline != null)
            selectionOutline.enabled = isSelected;
    }

    public void OnClick()
    {
        Debug.Log($"[CARD SLOT] Clicked card: {currentCard?.Data.symbolName}");
        uiController?.SelectCard(this);
    }


    public void Clear()
    {
        currentCard = null;
        icon.sprite = null;
        label.text = "";
        SetSelectedVisual(false);
    }
}
