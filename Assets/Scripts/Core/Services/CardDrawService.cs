using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Core.Services;

public class CardDrawService : ICardDrawService
{
    private List<SymbolDataSO> _symbolPool;
    private IGameStateService _stateService;
    private IDoomHandler _doomHandler;
    private SystemModifierService _modifierService;

    public CardDrawService(List<SymbolDataSO> symbolPool, IGameStateService stateService, IDoomHandler doomHandler, SystemModifierService modifierService)
    {
        _symbolPool = symbolPool;
        _stateService = stateService;
        _doomHandler = doomHandler;
        _modifierService = modifierService;
    }

    public SymbolDataSO GetWeightedSymbol()
    {
        if (_symbolPool == null || _symbolPool.Count == 0)
        {
            return null;
        }

        var bonuses = _modifierService.GetActiveBonuses();

        int totalWeight = 0;
        foreach (var symbol in _symbolPool)
        {
            string normalizedSymbolName = symbol.symbolName.ToLowerInvariant();
            int bonus = _modifierService.GetBonusForSymbol(normalizedSymbolName);
            totalWeight += symbol.drawWeight + bonus;
        }

        int roll = Random.Range(0, totalWeight);

        int cumulative = 0;
        foreach (var symbol in _symbolPool)
        {
            string normalizedSymbolName = symbol.symbolName.ToLowerInvariant();
            int bonus = _modifierService.GetBonusForSymbol(normalizedSymbolName);
            int adjustedWeight = symbol.drawWeight + bonus;

            cumulative += adjustedWeight;
            if (roll < cumulative)
            {
                return symbol;
            }
        }

        return _symbolPool[0];
    }

    public SymbolCard GenerateCard()
    {
        SymbolDataSO selected = GetWeightedSymbol();
        return new SymbolCard(selected);
    }

    public SymbolCard DrawSymbolCard(bool applyDoom = true)
    {
        SymbolCard newCard = GenerateCard();
        _stateService.AddCardToHand(newCard);

        if (applyDoom)
        {
            _stateService.AdvanceDraw();

            if (_doomHandler.TryTriggerDoom())
            {
                Debug.Log("DOOM TRIGGERED!");
            }
        }

        return newCard;
    }

    private void LogHand(string context)
    {
        // Removing debug logs for hand
    }

    public void InitializeSymbolPool(List<SymbolDataSO> symbols)
    {
        _symbolPool = symbols;
    }
}
