using UnityEngine;

[CreateAssetMenu(menuName = "GRIDLOCK/Tile Modifier")]
public class TileModifierSO : ScriptableObject
{
    public TileModifierType modifierType;
    public int amount; // Used for both flat value and multiplier types
}
