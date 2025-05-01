using UnityEngine;
using NewGameplay.Enums;

[CreateAssetMenu(fileName = "TileElement", menuName = "GRIDLOCK/TileElement")]
public class TileElementSO : ScriptableObject
{
    public TileElementType elementType;
    public float entropyChange;
    public Color tileColor = Color.white;
    public string description;
} 