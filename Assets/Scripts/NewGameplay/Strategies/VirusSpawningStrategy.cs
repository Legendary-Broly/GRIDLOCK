using System.Collections.Generic;
using UnityEngine;
using NewGameplay.Interfaces;
using System.Linq;

namespace NewGameplay.Strategies
{
    public class VirusSpawningStrategy
    {
        private readonly IVirusService virusService;
        private readonly System.Random random;
        private readonly int gridWidth;
        private readonly int gridHeight;
        private readonly string[,] gridState;
        private readonly bool[,] tilePlayable;

        // Virus configuration
        public const string VIRUS_SYMBOL = "X";
        public const string DATA_FRAGMENT_SYMBOL = "DATA";

        public VirusSpawningStrategy(
            IVirusService virusService,
            System.Random random,
            int gridWidth,
            int gridHeight,
            string[,] gridState,
            bool[,] tilePlayable)
        {
            this.virusService = virusService;
            this.random = random;
            this.gridWidth = gridWidth;
            this.gridHeight = gridHeight;
            this.gridState = gridState;
            this.tilePlayable = tilePlayable;
        }

        public List<Vector2Int> GetStaticVirusSpawnPositions(int count)
        {
            var result = new List<Vector2Int>();
            
            // Try to spawn near existing viruses first
            var nearVirusPositions = GetEmptyPositionsNearViruses();
            var processedSpots = new HashSet<Vector2Int>();
            
            // First add positions near viruses if available
            if (nearVirusPositions.Count > 0)
            {
                for (int i = 0; i < count && nearVirusPositions.Count > 0; i++)
                {
                    int index = random.Next(nearVirusPositions.Count);
                    var pos = nearVirusPositions[index];
                    
                    // Don't spawn on data fragments
                    if (gridState[pos.x, pos.y] == DATA_FRAGMENT_SYMBOL)
                    {
                        nearVirusPositions.RemoveAt(index);
                        i--; // Retry this iteration
                        continue;
                    }
                    
                    result.Add(pos);
                    processedSpots.Add(pos);
                    nearVirusPositions.RemoveAt(index);
                }
            }
            
            // If we need more positions, add random positions
            if (result.Count < count)
            {
                var emptyPositions = GetEmptyPositions(processedSpots);
                
                while (result.Count < count && emptyPositions.Count > 0)
                {
                    int index = random.Next(emptyPositions.Count);
                    var pos = emptyPositions[index];
                    
                    // Don't spawn on data fragments
                    if (gridState[pos.x, pos.y] == DATA_FRAGMENT_SYMBOL)
                    {
                        emptyPositions.RemoveAt(index);
                        continue;
                    }
                    
                    result.Add(pos);
                    emptyPositions.RemoveAt(index);
                }
            }
            
            return result;
        }

        private List<Vector2Int> GetEmptyPositionsNearViruses()
        {
            var result = new List<Vector2Int>();
            
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    if (gridState[x, y] == VIRUS_SYMBOL)
                    {
                        // Check adjacent positions
                        int[] dx = { 0, 1, 0, -1 };
                        int[] dy = { -1, 0, 1, 0 };
                        
                        for (int i = 0; i < 4; i++)
                        {
                            int nx = x + dx[i];
                            int ny = y + dy[i];
                            
                            if (IsInBounds(nx, ny) && gridState[nx, ny] != VIRUS_SYMBOL)
                            {
                                result.Add(new Vector2Int(nx, ny));
                            }
                        }
                    }
                }
            }
            
            return result;
        }
        
        private List<Vector2Int> GetEmptyPositions(HashSet<Vector2Int> processedSpots)
        {
            var result = new List<Vector2Int>();
            
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    var pos = new Vector2Int(x, y);
                    if (!processedSpots.Contains(pos) && 
                        string.IsNullOrEmpty(gridState[x, y]) && 
                        tilePlayable[x, y])
                    {
                        result.Add(pos);
                    }
                }
            }
            
            return result;
        }
        
        private bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
        }
    }
} 