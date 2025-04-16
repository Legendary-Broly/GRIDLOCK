using UnityEngine;
using System.Collections.Generic;

public class GameStateService : IGameStateService
{
    public List<SymbolCard> PlayerHand { get; } = new();
    public float CurrentMultiplier { get; private set; }
    public float CurrentDoomChance { get; private set; }

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
}
