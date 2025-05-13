using UnityEngine;

namespace NewGameplay.ScriptableObjects
{
    [CreateAssetMenu(fileName = "RoundConfig", menuName = "Gridlock/RoundConfig")]
    public class RoundConfigSO : ScriptableObject
    {
        public int roundNumber;
        public int gridWidth;
        public int gridHeight;
        public int fragmentRequirement;
        public int virusCount;
    }
}
