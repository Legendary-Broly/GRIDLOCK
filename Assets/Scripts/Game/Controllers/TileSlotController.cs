using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TileSlotController : MonoBehaviour
{
    [SerializeField] private Image symbolImage;
    [SerializeField] private TMP_Text symbolText;

    public SymbolCard CurrentCard { get; private set; }
    public bool IsOccupied => CurrentCard != null;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnTileClicked);
    }

    public void SetCard(SymbolCard card)
    {
        if (IsOccupied) return;

        CurrentCard = card;

        symbolImage.sprite = card.Data.symbolSprite;
        symbolImage.color = Color.white;

        if (symbolText != null)
            symbolText.text = card.Data.symbolName;

    }

    private void OnTileClicked()
    {

        if (IsOccupied) return;

        var ui = FindFirstObjectByType<GameplayUIController>();
        SymbolCard selectedCard = ui.ConsumeSelectedCard();

        if (selectedCard == null)
        {
            return;
        }

        SetCard(selectedCard);
    }
}
