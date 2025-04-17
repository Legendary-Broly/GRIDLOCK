using UnityEngine;
using System.Collections.Generic;

public class GameStateService : IGameStateService
{
    private float _currentDoomChance = 0f;
    private float _currentDoomMultiplier = 1f;
    private int _doomDrawCount = 0;
    private int _doomEffectCount = 0;

    private List<SymbolCard> _playerHand = new();

    public List<SymbolCard> PlayerHand => _playerHand;

    public float CurrentMultiplier => _currentDoomMultiplier;

    public int DoomEffectCount => _doomEffectCount;
    public List<SymbolBonus> ActiveDrinkBonuses { get; set; } = new();


    public float CurrentDoomChance => _currentDoomChance;
    public float CurrentDoomMultiplier => _currentDoomMultiplier;
    public int CurrentDoomStage => Mathf.Clamp(_doomEffectCount + 1, 1, 3);

    public void AddCardToHand(SymbolCard card)
    {
        _playerHand.Add(card);
    }

    public void AdvanceDraw()
    {
        _doomDrawCount++;

        _currentDoomChance = Mathf.Min(1f, _doomDrawCount * 0.1f);      // 10% per draw
        _currentDoomMultiplier = 1f + (_doomDrawCount * 0.25f);         // +0.25x per draw

        // Update DoomMeterUI with stage
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
}
