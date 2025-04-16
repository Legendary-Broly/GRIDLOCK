using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Unity.VisualScripting;

public class GameplayUIController : MonoBehaviour
{
    [Header("Draw Button")]
    [SerializeField] private Button drawButton;

    [Header("Doom UI")]
    [SerializeField] private Image doomBar;
    [SerializeField] private TMP_Text doomText;

    [Header("Multiplier UI")]
    [SerializeField] private Image multiplierBar;
    [SerializeField] private TMP_Text multiplierText;

    [Header("Hand Display")]
    [SerializeField] private Transform cardHandContainer;
    [SerializeField] private GameObject cardSlotPrefab;

    [Header("Grid")]
    [SerializeField] private GridManager gridManager;

    private const int MaxHandSize = 5;
    private IGridStateEvaluator gridStateEvaluator;
    private CardSlotController selectedCard = null;
    public void Init()
    {
        for (int i = 0; i < 3; i++)
            GameBootstrapper.CardDrawService.DrawSymbolCard(false); // Initial draw to populate the hand

        RebuildHandUI();
    }

    private void Start()
    {
        drawButton.onClick.AddListener(OnDrawButtonClicked);
        gridStateEvaluator = new GridStateEvaluator();

        RefreshUI();
    }


    public void OnDrawButtonClicked()
    {
        if (GameBootstrapper.GameStateService.PlayerHand.Count >= MaxHandSize)
        {
            // Debug.Log("Hand is full.");
            return;
        }

        GameBootstrapper.CardDrawService.DrawSymbolCard(true);
        RebuildHandUI(); // ✅ Trigger rebuild after draw
        RefreshUI();

    }
    private void CreateCardSlot(SymbolCard card)
    {
        GameObject slot = Instantiate(cardSlotPrefab, cardHandContainer);
        CardSlotController controller = slot.GetComponent<CardSlotController>();
        controller.Initialize(card, this);
        // Debug.Log($"[UI] CreateCardSlot called — card: {card.Data.symbolName}, from: {new System.Diagnostics.StackTrace()}");
    }

    private void RefreshUI()
    {
        float doom = GameBootstrapper.GameStateService.CurrentDoomChance;
        float mult = GameBootstrapper.GameStateService.CurrentDoomMultiplier;

        if (doomBar != null)
            doomBar.fillAmount = doom;

        if (doomText != null)
            doomText.text = $"Doom: {(int)(doom * 100)}%";

        if (multiplierBar != null)
            multiplierBar.fillAmount = mult / 4f;

        if (multiplierText != null)
            multiplierText.text = $"x{mult:0.0}";

        if (drawButton != null)
            drawButton.interactable = GameBootstrapper.GameStateService.PlayerHand.Count < MaxHandSize;
    }


    public void SelectCard(CardSlotController cardSlot)
    {
        if (selectedCard != null)
            selectedCard.SetSelectedVisual(false);

        selectedCard = cardSlot;
        selectedCard.SetSelectedVisual(true);
    }

    public SymbolCard ConsumeSelectedCard()
    {
        if (selectedCard == null) return null;

        SymbolCard card = selectedCard.Card;
        GameBootstrapper.GameStateService.PlayerHand.Remove(card);
        Destroy(selectedCard.gameObject);
        selectedCard = null;

        RefreshUI();
        return card;
    }
    public void OnGridlockPressed()
    {
        TileSlotController[,] grid = gridManager.GetTileGrid();
        float doomMultiplier = GameBootstrapper.GameStateService.CurrentMultiplier;

        int finalScore = gridStateEvaluator.EvaluateTotalScore(grid);
        // Debug.Log($"[GRIDLOCK] Final Score (includes Doom multiplier): {finalScore}");

    }
    public void RebuildHandUI()
    {
        // Forcefully destroy all children in the hand container
        foreach (Transform child in cardHandContainer)
        {
            Destroy(child.gameObject); // ✅ Forces full clean regardless of timing
        }

        var hand = GameBootstrapper.GameStateService.PlayerHand;
        foreach (var card in hand)
        {
            CreateCardSlot(card);
        }

        // Debug.Log($"[UI] Rebuilding hand: {hand.Count} cards");
    }

}
