using UnityEngine;

[CreateAssetMenu(fileName = "TileModifier", menuName = "GRIDLOCK/Tile Modifier")]
public class TileModifierSO : ScriptableObject
{
    public TileModifierType modifierType;  // AddValue or MultiplyValue
    public int amount;                     // 3, 5 for AddValue or 2, 3 for MultiplyValue

    public string GetDisplayText()
    {
        return modifierType switch
        {
            TileModifierType.AddValue => $"+{amount}",
            TileModifierType.MultiplyValue => $"x{amount}",
            _ => ""
        };
    }
}
