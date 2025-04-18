using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TileSlotController : MonoBehaviour
{
    [SerializeField] private Image symbolRenderer;  // ← was SpriteRenderer, now Image
    [SerializeField] private TMP_Text modifierText;

    private SymbolCard currentCard;
    private TileModifierSO modifier;

    public void SetCard(SymbolCard card)
    {
        if (currentCard != null) return;

        currentCard = card;
        symbolRenderer.sprite = card.Data.symbolSprite;
    }

    public bool HasSymbol()
    {
        return currentCard != null;
    }

    public string GetDisplayText()
    {
        return modifier != null ? modifier.GetDisplayText() : "";
    }

    public string GetSymbolName()
    {
        return currentCard != null ? currentCard.Data.symbolName : "None";
    }

    public int GetSymbolValue()
    {
        int baseValue = currentCard != null ? currentCard.Data.baseValue : 0;

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
    // Returns the active modifier (ScriptableObject)
    public TileModifierSO ActiveModifier => modifier;

    // Checks if the tile has any card or is disabled
    
    public bool IsOccupied()
    {
        return currentCard != null;
    }

    // Gets the card on the tile
    public SymbolCard CurrentCard => currentCard;

    // Force-sets a card even if already assigned
    public void ForceSetCard(SymbolCard card)
    {
        currentCard = card;
        symbolRenderer.sprite = card.Data.symbolSprite;
        RefreshVisuals();
    }

    // Marks tile visually unplayable
    public void MarkUnplayable()
    {
        symbolRenderer.color = Color.gray;
    }

    // Refresh visuals for tile state (can be expanded as needed)
    public void RefreshVisuals()
    {
        symbolRenderer.enabled = currentCard != null;
        modifierText.text = modifier != null ? modifier.GetDisplayText() : "";
    }
    public void OnClick()
    {
        if (isLocked || currentCard != null)
        {
            Debug.Log("[TILE CLICK] Cannot place: Tile is either locked or already occupied.");
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

    // Locks a tile to prevent further changes
    private bool isLocked = false;

    public bool IsLocked => isLocked;

    public void LockPermanently()
    {
        isLocked = true;
    }

}
