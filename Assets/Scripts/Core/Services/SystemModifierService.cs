using System.Collections.Generic;
using UnityEngine;

public class SymbolModifierService
{
    private Dictionary<string, int> bonusWeights = new();

    public void ApplyDrinkEffect(DrinkEffectSO drink)
    {
        if (bonusWeights.ContainsKey(drink.symbolNameToBoost))
            bonusWeights[drink.symbolNameToBoost] += drink.weightBonus;
        else
            bonusWeights[drink.symbolNameToBoost] = drink.weightBonus;

        Debug.Log($"[BAR PHASE] Boosted {drink.symbolNameToBoost} by +{drink.weightBonus} weight for next round.");
    }

    public int GetBonusForSymbol(string symbolName)
    {
        return bonusWeights.TryGetValue(symbolName, out int bonus) ? bonus : 0;
    }

    public void Reset()
    {
        bonusWeights.Clear();
    }
}
