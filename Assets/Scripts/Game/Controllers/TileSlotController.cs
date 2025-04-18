using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TileSlotController : MonoBehaviour
{
    [SerializeField] private Image symbolRenderer;
    [SerializeField] private TMP_Text modifierText;
    [SerializeField] private GameObject lockOverlay;

    private SymbolCard currentCard;
    private TileModifierSO modifier;

    public void SetCard(SymbolCard card)
    {
        if (currentCard != null) return;

        currentCard = card;
        symbolRenderer.sprite = card.Data.symbolSprite;
        RefreshVisuals();
    }

    public bool HasSymbol()
    {
        return currentCard != null;
    }

    public int GetSymbolValue()
    {
        if (currentCard == null || currentCard.Data == null)
        {
            Debug.LogWarning("[TileSlotController] GetSymbolValue: currentCard or its Data is null.");
            return 0;
        }

        int baseValue = currentCard.Data.baseValue;

        if (modifier != null)
        {
            switch (modifier.modifierType)
            {
                case TileModifierType.AddValue:
                    return baseValue + modifier.amount;
                case TileModifierType.MultiplyValue:
                    return baseValue * modifier.amount;
            }
        }

        return baseValue;
    }
    public string GetSymbolName()
    {
        if (currentCard == null || currentCard.Data == null)
            return "None";

        return currentCard.Data.symbolName;
    }

    public void AssignModifier(TileModifierSO mod)
    {
        modifier = mod;
        modifierText.text = modifier != null ? modifier.GetDisplayText() : "";
    }

    public TileModifierSO GetModifier()
    {
        return modifier;
    }

    public SymbolCard GetCard()
    {
        return currentCard;
    }

    public TileModifierSO ActiveModifier => modifier;
    public SymbolCard CurrentCard => currentCard;

    public bool IsOccupied()
    {
        return currentCard != null;
    }

    public bool IsUnplayable()
    {
        return lockOverlay != null && lockOverlay.activeSelf;
    }

    public bool IsPlayable()
    {
        return !IsUnplayable() && HasSymbol();
    }

    public void MarkTileUnplayable()
    {
        if (lockOverlay != null)
        {
            lockOverlay.SetActive(true);
        }

        symbolRenderer.color = Color.gray;
    }

    public void ForceSetCard(SymbolCard card)
    {
        currentCard = card;
        symbolRenderer.sprite = card.Data.symbolSprite;
        RefreshVisuals();
    }

    public void RefreshVisuals()
    {
        symbolRenderer.enabled = currentCard != null;
        modifierText.text = modifier != null ? modifier.GetDisplayText() : "";
    }

    public void OnClick()
    {
        if (IsUnplayable() || currentCard != null)
        {
            Debug.Log("[TILE CLICK] Cannot place: Tile is either unplayable or already occupied.");
            return;
        }

        var selected = GameplayUIController.Instance?.ConsumeSelectedCard();

        if (selected != null)
        {
            SetCard(selected);
            Debug.Log($"[TILE CLICK] Placed {selected.Data.symbolName} on {gameObject.name}");
        }
        else
        {
            Debug.Log("[TILE CLICK] No selected card available to place.");
        }
    }
}
