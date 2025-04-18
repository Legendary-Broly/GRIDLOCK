using UnityEngine;

[CreateAssetMenu(menuName = "GRIDLOCK/Symbol Data")]
public class SymbolDataSO : ScriptableObject
{
    public string symbolName;
    public Sprite symbolSprite;
    public int baseValue;
    public SymbolType symbolType;
    public bool triggersCombo;

    [Range(1, 100)]
    public int drawWeight = 1;
    public string displayName;
    public Sprite icon;
    
    // Add this property if it doesn't exist already
    [Tooltip("Base value used for scoring multiplier")]
    public int Value = 1;

}
public enum SymbolType
{
    None, // Added None as default value
    Fruit,
    Coin,
    Diamond,
    Seven
}
[System.Serializable]
public struct SymbolBonus
{
    public string symbolName;
    public int weightBonus;
}
