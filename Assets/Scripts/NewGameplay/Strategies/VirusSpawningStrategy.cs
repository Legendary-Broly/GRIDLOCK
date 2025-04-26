using System.Collections.Generic;
using UnityEngine;
using NewGameplay.Interfaces;
using NewGameplay.Configuration;
using System.Linq;

namespace NewGameplay.Strategies
{
    public class VirusSpawningStrategy
    {
        private readonly IEntropyService entropyService;
        private readonly System.Random rng;
        private readonly int gridWidth;
        private readonly int gridHeight;
        private readonly string[,] gridState;
        private readonly bool[,] tilePlayable;

        public VirusSpawningStrategy(
            IEntropyService entropyService,
            System.Random rng,
            int gridWidth,
            int gridHeight,
            string[,] gridState,
            bool[,] tilePlayable)
        {
            this.entropyService = entropyService;
            this.rng = rng;
            this.gridWidth = gridWidth;
            this.gridHeight = gridHeight;
            this.gridState = gridState;
            this.tilePlayable = tilePlayable;
        }

        public List<Vector2Int> GetVirusSpawnPositions()
        {
            if (entropyService == null)
            {
                Debug.LogError("EntropyService is null! Cannot determine virus spawn positions.");
                return new List<Vector2Int>();
            }

            var existingViruses = GetExistingViruses();
            var emptySpots = GetEmptySpots();
            var spawnPositions = new List<Vector2Int>();

            // Step 1: Handle virus growth - each existing virus spreads to one adjacent spot (can overwrite existing symbols)
            foreach (var virus in existingViruses)
            {
                var adjacentSpots = GetAdjacentSpreadableSpots(new List<Vector2Int> { virus });
                if (adjacentSpots.Count > 0)
                {
                    int index = rng.Next(adjacentSpots.Count);
                    spawnPositions.Add(adjacentSpots[index]);
                }
            }

            // Step 2: Handle new virus spawning based on entropy (only in empty spots)
            int newVirusCount = CalculateVirusCount();
            if (existingViruses.Count == 0 && emptySpots.Count > 0)
            {
                // If no viruses exist, spawn one in a random empty spot
                spawnPositions.Add(emptySpots[rng.Next(emptySpots.Count)]);
            }
            else
            {
                // Get all empty spots within 2 tiles of any existing virus
                var nearbyEmptySpots = GetEmptySpotsWithinRange(existingViruses, 2);
                
                // For new viruses, prefer nearby spots but fall back to any empty spot
                for (int i = 0; i < newVirusCount && (nearbyEmptySpots.Count > 0 || emptySpots.Count > 0); i++)
                {
                    List<Vector2Int> targetSpots = nearbyEmptySpots.Count > 0 ? nearbyEmptySpots : emptySpots;
                    int index = rng.Next(targetSpots.Count);
                    spawnPositions.Add(targetSpots[index]);
                    targetSpots.RemoveAt(index);
                }
            }

            return spawnPositions;
        }

        private List<Vector2Int> GetExistingViruses()
        {
            var viruses = new List<Vector2Int>();
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    if (gridState[x, y] == VirusConfiguration.VIRUS_SYMBOL)
                    {
                        viruses.Add(new Vector2Int(x, y));
                    }
                }
            }
            return viruses;
        }

        private List<Vector2Int> GetEmptySpots()
        {
            var emptySpots = new List<Vector2Int>();
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    if (tilePlayable[x, y])
                    {
                        emptySpots.Add(new Vector2Int(x, y));
                    }
                }
            }
            return emptySpots;
        }
        
        private List<Vector2Int> GetAdjacentSpreadableSpots(List<Vector2Int> viruses)
        {
            var adjacentSpots = new List<Vector2Int>();
            
            foreach (var virus in viruses)
            {
                foreach (var dir in VirusConfiguration.SPREAD_DIRECTIONS)
                {
                    int nx = virus.x + dir.x;
                    int ny = virus.y + dir.y;
                    
                    // Allow spreading to any cell that's in bounds (not just empty ones)
                    // but don't overwrite existing virus cells
                    if (IsInBounds(nx, ny) && gridState[nx, ny] != VirusConfiguration.VIRUS_SYMBOL)
                    {
                        adjacentSpots.Add(new Vector2Int(nx, ny));
                    }
                }
            }
            return adjacentSpots;
        }

        private List<Vector2Int> GetAdjacentEmptySpots(List<Vector2Int> viruses)
        {
            var adjacentSpots = new List<Vector2Int>();
            
            foreach (var virus in viruses)
            {
                foreach (var dir in VirusConfiguration.SPREAD_DIRECTIONS)
                {
                    int nx = virus.x + dir.x;
                    int ny = virus.y + dir.y;
                    if (IsInBounds(nx, ny) && tilePlayable[nx, ny])
                    {
                        adjacentSpots.Add(new Vector2Int(nx, ny));
                    }
                }
            }
            return adjacentSpots;
        }

        private int CalculateVirusCount()
        {
            int baseVirusCount = entropyService.EntropyPercent switch
            {
                < VirusConfiguration.LOW_ENTROPY_THRESHOLD => VirusConfiguration.LOW_ENTROPY_SPAWN_COUNT,
                < VirusConfiguration.MEDIUM_ENTROPY_THRESHOLD => VirusConfiguration.MEDIUM_ENTROPY_SPAWN_COUNT,
                _ => VirusConfiguration.HIGH_ENTROPY_SPAWN_COUNT
            };

            int growthRate = entropyService.GetVirusGrowthRate();
            int finalCount = baseVirusCount * growthRate;
            
            Debug.Log($"[VirusSpawning] Calculating virus count: Base count: {baseVirusCount}, Growth rate: {growthRate}, Final count: {finalCount}");
            
            return finalCount;
        }

        private List<Vector2Int> GetEmptySpotsWithinRange(List<Vector2Int> viruses, int range)
        {
            var nearbySpots = new List<Vector2Int>();
            var processedSpots = new HashSet<Vector2Int>();

            foreach (var virus in viruses)
            {
                for (int dx = -range; dx <= range; dx++)
                {
                    for (int dy = -range; dy <= range; dy++)
                    {
                        int nx = virus.x + dx;
                        int ny = virus.y + dy;
                        var pos = new Vector2Int(nx, ny);
                        
                        if (IsInBounds(nx, ny) && tilePlayable[nx, ny] && !processedSpots.Contains(pos))
                        {
                            nearbySpots.Add(pos);
                            processedSpots.Add(pos);
                        }
                    }
                }
            }

            return nearbySpots;
        }

        private bool IsInBounds(int x, int y) => x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
    }
} 