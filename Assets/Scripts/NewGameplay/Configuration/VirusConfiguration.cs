using UnityEngine;

namespace NewGameplay.Configuration
{
    public class VirusConfiguration
    {
        public const string VIRUS_SYMBOL = "X";
        
        // Entropy thresholds for virus spawn count
        public const int LOW_ENTROPY_THRESHOLD = 33;
        public const int MEDIUM_ENTROPY_THRESHOLD = 66;
        
        // Base virus spawn counts at different entropy levels
        public const int LOW_ENTROPY_SPAWN_COUNT = 1;
        public const int MEDIUM_ENTROPY_SPAWN_COUNT = 2;
        public const int HIGH_ENTROPY_SPAWN_COUNT = 3;

        // Directions for virus spreading
        public static readonly Vector2Int[] SPREAD_DIRECTIONS = new[]
        {
            new Vector2Int(1, 0),  // Right
            new Vector2Int(-1, 0), // Left
            new Vector2Int(0, 1),  // Up
            new Vector2Int(0, -1)  // Down
        };
    }
} 