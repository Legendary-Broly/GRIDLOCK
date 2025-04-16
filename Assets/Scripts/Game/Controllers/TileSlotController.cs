using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TileSlotController : MonoBehaviour
{
    [SerializeField] private Image symbolImage;
    [SerializeField] private TMP_Text symbolText;
    [SerializeField] private TMP_Text modifierText;
    [SerializeField] private TMP_Text modifierIndicatorText;


    private bool isOccupied = false;
    private SymbolCard currentCard;
    private TileModifierSO modifier;
    public SymbolCard CurrentCard => currentCard;
    public TileModifierSO ActiveModifier => modifier;
    public TileModifierSO GetModifier()
    {
        return modifier;
    }

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnTileClicked);
        modifierText.text = "";
        modifierText.gameObject.SetActive(false); // hides the text at startup
    }

    public void SetCard(SymbolCard card)
    {
        if (isOccupied) return;

        currentCard = card;
        isOccupied = true;

        symbolImage.sprite = card.Data.symbolSprite;
        symbolImage.color = Color.white;

        if (symbolText != null)
            symbolText.text = card.Data.symbolName;

        // Debug.Log("[TileSlot] Card placed: " + card.Data.symbolName);
    }

    public bool IsOccupied() => isOccupied;
    public void ForceSetCard(SymbolCard card)
    {
        currentCard = card;
        isOccupied = true;
        RefreshVisuals();
    }

    public void AssignModifier(TileModifierSO modifierSO)
    {
        modifier = modifierSO;

        if (modifierSO == null)
        {
            modifierText.gameObject.SetActive(false);
            // Debug.Log("Modifier cleared or null.");
            return;
        }

        string modDisplay = modifierSO.modifierType switch
        {
            TileModifierType.AddValue => $"+{modifierSO.amount}",
            TileModifierType.MultiplyValue => $"x{modifierSO.amount}",
            _ => ""
        };

        modifierText.text = modDisplay;
        modifierText.gameObject.SetActive(true);

        // Debug.Log($"Modifier text set to: {modDisplay}");
    }

    public void OnTileClicked()
    {
        if (isOccupied) return;

        var ui = FindFirstObjectByType<GameplayUIController>();
        SymbolCard card = ui.ConsumeSelectedCard();
        if (card == null) return;

        SetCard(card);
    }
    public int GetModifiedSymbolValue()
    {
        if (currentCard == null) return 0;

        int baseValue = currentCard.Data.baseValue;
        if (modifier == null) return baseValue;

        return modifier.modifierType switch
        {
            TileModifierType.AddValue => baseValue + modifier.amount,
            TileModifierType.MultiplyValue => baseValue * modifier.amount,
            _ => baseValue
        };
    }
    public void MarkUnplayable()
    {
        isOccupied = true;
        modifierText.text = "X"; // Or "🕳️" or "DEAD"
        modifierText.color = Color.red;
        modifierText.gameObject.SetActive(true);
    }

    public void LockPermanently()
    {
        isOccupied = true;
        modifierText.text = "🔒";
    }

    public bool IsLocked() => isOccupied;

    public void RefreshVisuals()
    {
        if (currentCard != null)
        {
            symbolImage.sprite = currentCard.Data.symbolSprite;
            symbolText.text = currentCard.Data.symbolName;
        }
    }
}
