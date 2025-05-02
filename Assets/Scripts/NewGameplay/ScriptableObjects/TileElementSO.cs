using UnityEngine;
using NewGameplay.Enums;

namespace NewGameplay
{
    [CreateAssetMenu(fileName = "TileElement", menuName = "TileElements/TileElement")]
    public class TileElementSO : ScriptableObject
    {
        public TileElementType elementType;
        public int entropyChange;
        public float progressPercent;
        public string displayText;
        public Color displayColor = Color.white;  // Optional for styling
        public string description;  // Added from Models version
    }
}
