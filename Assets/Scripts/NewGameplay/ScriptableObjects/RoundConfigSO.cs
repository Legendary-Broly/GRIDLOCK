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

        [Header("Split Grid Settings")]
        public bool useSplitGrid;
        public int gridAWidth;
        public int gridAHeight;
        public int gridBWidth;
        public int gridBHeight;
        public int virusCountA;
        public int virusCountB;
        public int fragmentRequirementA;
        public int fragmentRequirementB;
    }
}
