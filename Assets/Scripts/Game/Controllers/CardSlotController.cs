using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardSlotController : MonoBehaviour
{
    public SymbolCard Card { get; private set; }

    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text label;

    private Button button;
    private GameplayUIController uiController;

    public void Initialize(SymbolCard card, GameplayUIController controller)
    {
        Card = card;
        icon.sprite = card.Data.symbolSprite;
        label.text = card.Data.symbolName;
        uiController = controller;

        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        uiController.SelectCard(this);
    }

    public void SetSelectedVisual(bool selected)
    {
        icon.color = selected ? Color.yellow : Color.white;
    }
}
