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

}
public enum SymbolType
{
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
