using System.Collections.Generic;
using Unity.VisualScripting;
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
        if (_symbolPool == null || _symbolPool.Count == 0)
        {
            Debug.LogWarning("Symbol pool is empty or not initialized.");
            return null; // Or throw an exception if this is critical
        }

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
    public SymbolCard GenerateCard()
    {
        SymbolDataSO selected = GetWeightedSymbol();
        return new SymbolCard(selected);
    }

    public SymbolCard DrawSymbolCard(bool applyDoom = true)
    {
        SymbolCard newCard = GenerateCard();
        _stateService.AddCardToHand(newCard);
        LogHand("After draw");

        if (applyDoom)
        {
            _stateService.AdvanceDraw();

            if (_doomHandler.TryTriggerDoom())
            {
                Debug.Log("DOOM TRIGGERED!");
                // DO NOT add another card here
            }
        }

        return newCard;
    }

    public SymbolDataSO GetRandomSymbol()
    {
        return _symbolPool[Random.Range(0, _symbolPool.Count)];
    }
    private void LogHand(string context)
    {
        if (_stateService?.PlayerHand == null)
        {
            Debug.LogWarning("Player hand is not initialized.");
            return;
        }

        var hand = _stateService.PlayerHand;
        Debug.Log($"[HAND] {context} ({hand.Count} cards):");
        foreach (var c in hand)
            Debug.Log($" - {c.Data.symbolName}");
    }
    public void InitializeSymbolPool(List<SymbolDataSO> symbols)
    {
        _symbolPool = symbols;
    }

}
