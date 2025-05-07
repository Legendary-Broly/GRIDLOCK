using UnityEngine;
using NewGameplay.Enums;

namespace NewGameplay.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NewTileElement", menuName = "Gridlock/Tile Element")]
    public class TileElementSO : ScriptableObject
    {
        public TileElementType elementType;
        public string displayName;
        public string description;
        public Sprite icon;
        public Color displayColor;
        public string displayText;
    }
}
