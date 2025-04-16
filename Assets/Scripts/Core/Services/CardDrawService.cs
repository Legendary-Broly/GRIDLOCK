using System.Collections.Generic;
using UnityEngine;

public class CardDrawService : ICardDrawService
{
    private List<SymbolDataSO> _symbolPool;
    private IGameStateService _stateService;
    private IDoomHandler _doomHandler;

    public CardDrawService(List<SymbolDataSO> symbolPool, IGameStateService stateService, IDoomHandler doomHandler)
    {
        _symbolPool = symbolPool;
        _stateService = stateService;
        _doomHandler = doomHandler;
    }

    private SymbolDataSO GetWeightedSymbol()
    {
        int totalWeight = 0;

        foreach (var symbol in _symbolPool)
            totalWeight += symbol.drawWeight;

        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;

        foreach (var symbol in _symbolPool)
        {
            cumulative += symbol.drawWeight;
            if (roll < cumulative)
                return symbol;
        }

        // Fallback
        return _symbolPool[0];
    }

    public SymbolCard DrawSymbolCard(bool applyDoom = true)
    {
        SymbolDataSO selectedSymbol = GetWeightedSymbol();
        SymbolCard newCard = new(selectedSymbol);

        _stateService.AddCardToHand(newCard);

        if (applyDoom)
        {
            _stateService.AdvanceDraw();

            if (_doomHandler.TryTriggerDoom())
            {
                Debug.Log("DOOM TRIGGERED! Add corrupted card here.");
            }
        }

        return newCard;
    }


}
