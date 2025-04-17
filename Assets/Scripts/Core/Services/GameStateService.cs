using UnityEngine;
using System.Collections.Generic;

public class GameStateService : MonoBehaviour, IGameStateService
{
    public static GameStateService Instance { get; private set; }

    private float _currentDoomChance = 0f;
    private float _currentDoomMultiplier = 1f;
    private int _doomDrawCount = 0;
    private int _doomEffectCount = 0;
    
    // Unified grid size field with a private setter
    private int _gridSize = 3;
    public int CurrentGridSize 
    { 
        get => _gridSize;
        set => _gridSize = value;
    }

    private List<SymbolCard> _playerHand = new();
    public List<SymbolCard> PlayerHand => _playerHand;
    public float CurrentMultiplier => _currentDoomMultiplier;
    public int DoomEffectCount => _doomEffectCount;
    public List<SymbolBonus> ActiveDrinkBonuses { get; set; } = new();
    public float CurrentDoomChance => _currentDoomChance;
    public float CurrentDoomMultiplier => _currentDoomMultiplier;
    public int CurrentDoomStage => Mathf.Clamp(_doomEffectCount + 1, 1, 3);

    private int[] gridSizes = new int[] { 3, 4, 5 };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddCardToHand(SymbolCard card)
    {
        _playerHand.Add(card);
    }

    public void AdvanceDraw()
    {
        _doomDrawCount++;

        _currentDoomChance = Mathf.Min(1f, _doomDrawCount * 0.1f);      // 10% per draw
        _currentDoomMultiplier = 1f + (_doomDrawCount * 0.25f);         // +0.25x per draw

        GameBootstrapper.DoomMeterUI.UpdateDoomMeter(
            _currentDoomChance,
            _currentDoomMultiplier,
            CurrentDoomStage
        );
    }

    public void IncrementDoomEffectCount()
    {
        _doomEffectCount++;
    }

    public int GetCurrentGridSize()
    {
        return _gridSize;
    }

    public void AdvanceGridSize()
    {
        if (_gridSize == 3)
            _gridSize = 4;
        else if (_gridSize == 4)
            _gridSize = 5;
        else
            _gridSize = 3;

        Debug.Log($"[GAME STATE] Grid size advanced to {_gridSize}x{_gridSize}");
    }
}
