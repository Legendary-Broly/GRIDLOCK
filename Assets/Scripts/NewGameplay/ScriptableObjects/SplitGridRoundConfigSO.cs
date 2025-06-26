using UnityEngine;

namespace NewGameplay.ScriptableObjects
{
    [CreateAssetMenu(fileName = "SplitGridRoundConfig", menuName = "Gridlock/SplitGridRoundConfig")]
    public class SplitGridRoundConfigSO : ScriptableObject
    {
        public int roundNumber;
        public int gridWidth;
        public int gridHeight;
        public int fragmentRequirement;
        public int virusCount;
    }
}
