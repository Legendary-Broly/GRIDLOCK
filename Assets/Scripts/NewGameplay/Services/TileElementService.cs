using System.Collections.Generic;
using UnityEngine;
using NewGameplay.Interfaces;
using NewGameplay.Enums;
using System.Linq;
using NewGameplay.Models;
using NewGameplay.ScriptableObjects;

namespace NewGameplay.Services
{
    public class TileElementService : ITileElementService
    {
        private TileElementType[,] gridElements;
        private int gridWidth;
        private int gridHeight;
        private readonly List<TileElementSO> elementConfigs;
        private IGridService gridService;
        private Vector2Int? virusNestPosition = null;

        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;

        public TileElementService(
            int initialWidth,
            int initialHeight,
            List<TileElementSO> configs)
        {
            this.gridWidth = initialWidth;
            this.gridHeight = initialHeight;
            this.elementConfigs = configs;
            // Initial array is NOT created here — ResizeGrid() must be called
        }

        public void SetGridService(IGridService service)
        {
            this.gridService = service;
        }

        public void ResizeGrid(int width, int height)
        {
            gridWidth = width;
            gridHeight = height;
            gridElements = new TileElementType[gridWidth, gridHeight];
            GenerateElements();
        }

        public void GenerateElements()
        {
            for (int y = 0; y < gridHeight; y++)
                for (int x = 0; x < gridWidth; x++)
                    gridElements[x, y] = TileElementType.Empty;

            List<Vector2Int> positions = new List<Vector2Int>();
            for (int y = 0; y < gridHeight; y++)
                for (int x = 0; x < gridWidth; x++)
                    positions.Add(new Vector2Int(x, y));

            for (int i = positions.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                var temp = positions[i];
                positions[i] = positions[j];
                positions[j] = temp;
            }

            int currentPosition = 0;
            var elementCounts = new Dictionary<TileElementType, int>
            {
                { TileElementType.VirusNest, 0 },
                { TileElementType.CodeShard, 0 },
            };

            foreach (var elementCount in elementCounts)
            {
                for (int i = 0; i < elementCount.Value && currentPosition < positions.Count; i++)
                {
                    var pos = positions[currentPosition];
                    gridElements[pos.x, pos.y] = elementCount.Key;

                    if (elementCount.Key == TileElementType.VirusNest)
                    {
                        virusNestPosition = new Vector2Int(pos.x, pos.y);
                    }

                    currentPosition++;
                }
            }
        }

        public void TriggerElementEffect(int x, int y)
        {
            var element = gridElements[x, y];
            var config = elementConfigs.FirstOrDefault(e => e.elementType == element);

            if (config == null)
            {
                Debug.LogWarning($"[TileElementService] No configuration found for element type {element} at ({x},{y})");
                return;
            }

            switch (element)
            {
                case TileElementType.CodeShard:
                    Debug.Log("Code shard found.");
                    break;

                case TileElementType.VirusNest:
                    Debug.Log("[TileElementService] Virus Nest triggered — spawning virus.");
                    gridElements[x, y] = TileElementType.Empty;
                    break;
            }
        }

        public void TriggerElementEffectForFirstVirus()
        {
            if (virusNestPosition.HasValue)
            {
                Vector2Int pos = virusNestPosition.Value;
                Debug.Log($"[TileElementService] Spawning virus from nest at {pos}");
                gridElements[pos.x, pos.y] = TileElementType.Empty;
            }
            else
            {
                Debug.LogWarning("[TileElementService] No virus nest position found.");
            }
        }

        public Vector2Int? GetVirusNestPosition() => virusNestPosition;

        public TileElementType GetElementAt(int x, int y)
        {
            if (x < 0 || y < 0 || x >= gridElements.GetLength(0) || y >= gridElements.GetLength(1))
            {
                Debug.LogWarning($"[TileElementService] GetElementAt({x},{y}) out of bounds — grid size: {gridElements.GetLength(0)}x{gridElements.GetLength(1)}");
                return TileElementType.Empty;
            }

            return gridElements[x, y];
        }

        public TileElementSO GetElementSOAt(int x, int y)
        {
            var type = gridElements[x, y];
            return elementConfigs.FirstOrDefault(e => e.elementType == type);
        }
    }
}
