using UnityEngine;
using System.Collections.Generic;
using NewGameplay.ScriptableObjects;

namespace SplitGrid.Data
{
    [CreateAssetMenu(fileName = "SplitRoundConfigDatabase", menuName = "GRIDLOCK/Split Round Config Database")]
    public class SplitRoundConfigDatabase : ScriptableObject
    {
        [System.Serializable]
        public class SplitRoundConfig : RoundConfigSO
        {
            public bool useSplitGrid = true;
            public int gridAWidth = 7;
            public int gridAHeight = 7;
            public int gridBWidth = 7;
            public int gridBHeight = 7;
            public int gridAFragmentRequirement = 1;
            public int gridBFragmentRequirement = 1;
            public int gridAVirusCount = 2;
            public int gridBVirusCount = 2;
        }

        [SerializeField] private List<SplitRoundConfig> roundConfigs = new List<SplitRoundConfig>();

        public SplitRoundConfig GetConfigForRound(int round)
        {
            if (round < 0 || round >= roundConfigs.Count)
            {
                Debug.LogWarning($"No split round config found for round {round}. Using default config.");
                return new SplitRoundConfig
                {
                    useSplitGrid = true,
                    gridAWidth = 7,
                    gridAHeight = 7,
                    gridBWidth = 7,
                    gridBHeight = 7,
                    gridAFragmentRequirement = 1,
                    gridBFragmentRequirement = 1,
                    gridAVirusCount = 2,
                    gridBVirusCount = 2
                };
            }

            return roundConfigs[round];
        }

        public bool IsSplitGridRound(int round)
        {
            var config = GetConfigForRound(round);
            return config.useSplitGrid;
        }
    }
} 