using UnityEngine;
using System.Collections.Generic;

public class GameStateService : IGameStateService
{
    public List<SymbolCard> PlayerHand { get; } = new();
    public float CurrentMultiplier { get; private set; }
    public float CurrentDoomChance { get; private set; }
    private float _currentDoomMultiplier = 1.0f;
    public float CurrentDoomMultiplier => _currentDoomMultiplier;

    private GameConfigSO _config;

    public GameStateService(GameConfigSO config)
    {
        _config = config;
        CurrentMultiplier = _config.multiplierStart;
        CurrentDoomChance = _config.doomStartChance;
    }

    public void AddCardToHand(SymbolCard card)
    {
        PlayerHand.Add(card);
    }

    public void AdvanceDraw()
    {
        CurrentMultiplier = Mathf.Min(CurrentMultiplier + _config.multiplierIncrementPerDraw, _config.multiplierMax);
        CurrentDoomChance = Mathf.Min(CurrentDoomChance + _config.doomIncrementPerDraw, _config.doomMaxChance);
    }

    private int CalculateDoomStage(float doomChance)
    {
        if (doomChance < 0.2f) return 1;
        if (doomChance < 0.4f) return 2;
        return 3;
    }
    private int _doomEffectCount = 0;

    public int DoomEffectCount => _doomEffectCount;

    public void IncrementDoomEffectCount()
    {
        _doomEffectCount++;
    }

    public int CurrentDoomStage => _doomEffectCount switch
    {
        0 => 1,
        1 => 2,
        _ => 3
    };

}
