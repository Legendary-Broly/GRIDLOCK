using System.Collections.Generic;
using UnityEngine;
using NewGameplay.Interfaces;
using NewGameplay.Enums;
using System.Linq;
using NewGameplay.Models;
using NewGameplay.ScriptableObjects;
using NewGameplay.Views;
using NewGameplay.Controllers;
using NewGameplay.Services;

namespace NewGameplay.Services
{
    public class TileElementService : ITileElementService
    {
        private TileElementType[,] gridElements;
        private int gridWidth;
        private int gridHeight;
        private readonly List<TileElementSO> elementConfigs;
        private IGridService gridService;
        private NewGameplayBootstrapper bootstrapper;
        private IInjectService injectService;
        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;
        private ISystemIntegrityService systemIntegrityService;
        private ICodeShardTracker codeShardTracker;
        private IChatLogService chatLogService;

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
        public void SetSystemIntegrityService(ISystemIntegrityService service)
        {
            this.systemIntegrityService = service;
        }

        public void SetGridService(IGridService service)
        {
            this.gridService = service;
        }
        public void SetCodeShardTracker(ICodeShardTracker tracker)
        {
            this.codeShardTracker = tracker;
        }

        public void SetInjectService(IInjectService service)
        {
            this.injectService = service;
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
            // Clear all existing elements
            for (int y = 0; y < gridHeight; y++)
                for (int x = 0; x < gridWidth; x++)
                    gridElements[x, y] = TileElementType.Empty;

            // Collect valid tile positions (no viruses)
            List<Vector2Int> candidatePositions = new List<Vector2Int>();
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    if (gridService == null || !gridService.IsInBounds(x, y)) continue;

                    string symbol = gridService.GetSymbolAt(x, y);
                    bool isVirus = symbol == "X";
                    bool isEmpty = string.IsNullOrEmpty(symbol);
                    var tileState = gridService.GetTileState(x, y);
                    bool isHidden = tileState == TileState.Hidden;

                    if (isEmpty && !isVirus && isHidden)
                    {
                        candidatePositions.Add(new Vector2Int(x, y));
                    }

                }
            }

            // Shuffle candidate positions
            for (int i = candidatePositions.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (candidatePositions[i], candidatePositions[j]) = (candidatePositions[j], candidatePositions[i]);
            }

            int tilesPerElement = Mathf.FloorToInt(candidatePositions.Count * 0.1f); // 5%

            var elementTypes = new[]
            {
                TileElementType.CodeShard,
                TileElementType.SystemIntegrityIncrease,
                TileElementType.ToolRefresh
            };

            int posIndex = 0;

            foreach (var element in elementTypes)
            {
                for (int i = 0; i < tilesPerElement && posIndex < candidatePositions.Count; i++, posIndex++)
                {
                    var pos = candidatePositions[posIndex];
                    gridElements[pos.x, pos.y] = element;
                }
            }

            Debug.Log($"[TileElementService] Generated {tilesPerElement} of each element type across {candidatePositions.Count} eligible tiles.");
        }

        public void TriggerElementEffect(int x, int y)
        {
            Debug.Log($"[TriggerElementEffect] Called at ({x},{y}) — Element: {gridElements[x, y]}");

            var element = gridElements[x, y];
            var config = elementConfigs.FirstOrDefault(e => e.elementType == element);

            if (config == null)
            {
                Debug.Log("[TileElementService] Empty tile element at ({x},{y})");
                return;
            }

            switch (element)
            {
                case TileElementType.CodeShard:
                    if (codeShardTracker != null)
                    {
                        codeShardTracker.AddShard();
                        Debug.Log("[TileElement] 🧩 Code Shard collected on reveal.");
                    }
                    else
                    {
                        Debug.LogWarning("[TileElement] CodeShardTracker is null.");
                    }
                    break;

                case TileElementType.SystemIntegrityIncrease:
                    if (systemIntegrityService != null)
                    {
                        float missing = 100f - systemIntegrityService.CurrentIntegrity;
                        float restore = missing * 0.25f;
                        systemIntegrityService.Increase(restore);
                        Debug.Log($"[TileElement] System Integrity raised by {restore} (25% of missing)");
                    }
                    else
                    {
                        Debug.LogWarning("[TileElement] SystemIntegrityService is null.");
                    }
                    break;
                    
                case TileElementType.ToolRefresh:
                    if (injectService != null)
                    {
                        injectService.ResetForNewRound();
                        Debug.Log("[TileElement] Tool uses refreshed.");
                    }
                    else
                    {
                        Debug.LogWarning("[TileElement] InjectService is null.");
                    }
                    break;
            }
            chatLogService.LogTileElementReveal(element);
        }

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
        public void SetChatLogService(IChatLogService service)
        {
            chatLogService = service;
        }
    }
}