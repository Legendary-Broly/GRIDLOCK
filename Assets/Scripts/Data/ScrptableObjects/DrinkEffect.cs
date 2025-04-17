using UnityEngine;

[CreateAssetMenu(menuName = "BarPhase/Drink Effect")]
public class DrinkEffectSO : ScriptableObject
{
    public string displayName;
    public string symbolNameToBoost; // e.g., "Grape"
    public int weightBonus = 1;
    public DrinkRarity rarity;
}

public enum DrinkRarity
{
    Common,
    Uncommon,
    Rare,
    VeryRare
}
