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
        private readonly int gridSize;
        private readonly string[,] gridState;
        private readonly bool[,] tilePlayable;

        public VirusSpawningStrategy(
            IEntropyService entropyService,
            System.Random rng,
            int gridSize,
            string[,] gridState,
            bool[,] tilePlayable)
        {
            this.entropyService = entropyService;
            this.rng = rng;
            this.gridSize = gridSize;
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
            var adjacentEmptySpots = GetAdjacentEmptySpots(existingViruses);

            // Calculate how many viruses to spawn
            int virusCount = CalculateVirusCount();

            // If no viruses exist, spawn one in a random empty spot
            if (existingViruses.Count == 0 && emptySpots.Count > 0)
            {
                return new List<Vector2Int> { emptySpots[rng.Next(emptySpots.Count)] };
            }

            // Remove duplicates from adjacent spots
            adjacentEmptySpots = adjacentEmptySpots.Distinct().ToList();

            // Select positions for new viruses
            var spawnPositions = new List<Vector2Int>();
            for (int i = 0; i < virusCount && adjacentEmptySpots.Count > 0; i++)
            {
                int index = rng.Next(adjacentEmptySpots.Count);
                spawnPositions.Add(adjacentEmptySpots[index]);
                adjacentEmptySpots.RemoveAt(index);
            }

            return spawnPositions;
        }

        private List<Vector2Int> GetExistingViruses()
        {
            var viruses = new List<Vector2Int>();
            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
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
            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    if (tilePlayable[x, y])
                    {
                        emptySpots.Add(new Vector2Int(x, y));
                    }
                }
            }
            return emptySpots;
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

            return baseVirusCount * entropyService.GetVirusGrowthRate();
        }

        private bool IsInBounds(int x, int y) => x >= 0 && x < gridSize && y >= 0 && y < gridSize;
    }
} 