using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

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
    [SerializeField] private ScoreBreakdownUI scoreBreakdownUI;
    [SerializeField] private TextMeshProUGUI doomEffectText;
    public static GameplayUIController Instance { get; private set; }

    private const int MaxHandSize = 5;
    private CardSlotController selectedCard = null;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ShowDoomEffect(string effectDescription)
    {
        doomEffectText.text = $"Doom Effect: {effectDescription}";
        StopAllCoroutines();
        StartCoroutine(FadeDoomText());
    }

    private IEnumerator FadeDoomText()
    {
        yield return new WaitForSeconds(3f);
        doomEffectText.text = "";
    }

    public void Init()
    {
        if (GameBootstrapper.CardDrawService == null ||
            GameBootstrapper.GameStateService == null ||
            GameBootstrapper.GridManager == null)
        {
            Debug.LogError("One or more GameBootstrapper services are null in Init");
            return;
        }

        StartNewRound();

        Debug.Log($"[UI CONTROLLER] Init with grid size {GameBootstrapper.GameStateService.CurrentGridSize}");
        GameBootstrapper.GridManager.GenerateGridFromState();
    }

    private void Start()
    {
        drawButton.onClick.AddListener(OnDrawButtonClicked);

        if (PlayerPrefs.GetInt("ResetGameState", 0) == 1)
        {
            PlayerPrefs.SetInt("ResetGameState", 0);
            PlayerPrefs.Save();
            StartNewRound();
        }

        RefreshUI();
    }

    public void OnDrawButtonClicked()
    {
        if (GameBootstrapper.GameStateService.PlayerHand.Count >= MaxHandSize)
            return;

        GameBootstrapper.CardDrawService.DrawSymbolCard(true);
        RebuildHandUI();
        RefreshUI();
    }

    private void CreateCardSlot(SymbolCard card)
    {
        GameObject slot = Instantiate(cardSlotPrefab, cardHandContainer);
        CardSlotController controller = slot.GetComponent<CardSlotController>();
        controller.Initialize(card, this);
    }

    private void RefreshUI()
    {
        if (GameBootstrapper.GameStateService == null) return;

        float doom = GameBootstrapper.GameStateService.CurrentDoomChance;
        float mult = GameBootstrapper.GameStateService.CurrentDoomMultiplier;

        if (doomBar != null) doomBar.fillAmount = doom;
        if (doomText != null) doomText.text = $"Doom: {(int)(doom * 100)}%";
        if (multiplierBar != null) multiplierBar.fillAmount = mult / 4f;
        if (multiplierText != null) multiplierText.text = $"x{mult:0.0}";
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
        var grid = gridManager.GetTileGrid();

        if (grid == null)
        {
            Debug.LogError("[GRIDLOCK] Tile grid is null! Cannot calculate score.");
            return;
        }

        int finalScore = ScoreManager.Instance.CalculateTotalScore(grid);
        int baseScore = ScoreManager.Instance.RawScore(grid);
        string summary = ScoreManager.Instance.GridStateSummary(grid);
        float doomMultiplier = GameBootstrapper.GameStateService.CurrentDoomMultiplier;

        scoreBreakdownUI.ShowBreakdown(baseScore, summary, doomMultiplier, finalScore);
    }


    public void RebuildHandUI()
    {
        if (cardHandContainer == null) return;

        foreach (Transform child in cardHandContainer)
            Destroy(child.gameObject);

        if (GameBootstrapper.GameStateService == null) return;

        foreach (var card in GameBootstrapper.GameStateService.PlayerHand)
            CreateCardSlot(card);
    }

    public void StartNewRound()
    {
        if (GameBootstrapper.GameStateService == null) return;

        GameBootstrapper.GameStateService.ResetDoomState();
        GameBootstrapper.GameStateService.ResetPlayerHand();

        for (int i = 0; i < 3; i++)
            GameBootstrapper.CardDrawService.DrawSymbolCard(false);

        RebuildHandUI();

        if (GameBootstrapper.DoomMeterUI != null)
        {
            GameBootstrapper.DoomMeterUI.UpdateDoomMeter(
                GameBootstrapper.GameStateService.CurrentDoomChance,
                GameBootstrapper.GameStateService.CurrentDoomMultiplier,
                GameBootstrapper.GameStateService.CurrentDoomStage
            );
        }

        RefreshUI();
    }
}
