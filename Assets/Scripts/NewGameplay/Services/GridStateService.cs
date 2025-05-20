using System;
using UnityEngine;
using NewGameplay.Models;
using NewGameplay.Interfaces;
using NewGameplay.Controllers;
using System.Collections.Generic;
using NewGameplay.Enums;

namespace NewGameplay.Services
{
    public class GridStateService : IGridStateService
    {
        private string[,] gridState;
        private TileState[,] tileStates;
        private bool[,] virusFlags;
        private int gridWidth;
        private int gridHeight;
        private PayloadManager payloadManager;
        private HashSet<Vector2Int> echoRetentionTiles = new();
        public event Action OnGridStateChanged;
        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;
        private IGridService gridService;

        public void SetPayloadManager(PayloadManager manager) => payloadManager = manager;

        public void SetGridSize(int width, int height)
        {
            // Store old dimensions
            int oldWidth = gridWidth;
            int oldHeight = gridHeight;

            gridWidth = width;
            gridHeight = height;

            // Create new arrays
            var newGridState = new string[width, height];
            var newTileStates = new TileState[width, height];
            var newVirusFlags = new bool[width, height];

            // Initialize new arrays
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    newGridState[x, y] = null;
                    newTileStates[x, y] = TileState.Hidden;
                    newVirusFlags[x, y] = false;
                }
            }

            // Copy over existing data where possible
            if (gridState != null)
            {
                for (int y = 0; y < Mathf.Min(oldHeight, height); y++)
                {
                    for (int x = 0; x < Mathf.Min(oldWidth, width); x++)
                    {
                        newGridState[x, y] = gridState[x, y];
                        newTileStates[x, y] = tileStates[x, y];
                        newVirusFlags[x, y] = virusFlags[x, y];
                    }
                }
            }

            // Update arrays
            gridState = newGridState;
            tileStates = newTileStates;
            virusFlags = newVirusFlags;

            OnGridStateChanged?.Invoke();
        }

        public void ClearAllTiles()
        {
            PrecomputeEchoTiles();

            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    Vector2Int pos = new(x, y);
                    // Always clear virus flags
                    virusFlags[x, y] = false;
                    
                    if (echoRetentionTiles.Contains(pos))
                    {
                        Debug.Log($"[Echo] Preserving revealed state at ({x},{y}) but resetting contents");
                        tileStates[x, y] = TileState.Revealed; // Keep revealed state
                        gridState[x, y] = null; // Reset the contents
                        continue;
                    }
                    tileStates[x, y] = TileState.Hidden;
                    gridState[x, y] = null;
                }
            }

            OnGridStateChanged?.Invoke();
        }

        public void SetTileState(int x, int y, TileState state)
        {
            if (!IsValidTile(x, y)) return;
            tileStates[x, y] = state;
            OnGridStateChanged?.Invoke();
        }

        public TileState GetTileState(int x, int y)
        {
            if (!IsValidTile(x, y)) return TileState.Hidden;
            return tileStates[x, y];
        }

        public void SetGridState(int x, int y, string symbol)
        {
            if (!IsValidTile(x, y)) return;
            gridState[x, y] = symbol;
        }

        public string GetGridState(int x, int y)
        {
            if (!IsValidTile(x, y)) return null;
            return gridState[x, y];
        }

        private bool IsValidTile(int x, int y)
        {
            return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
        }
        public string GetSymbolAt(int x, int y)
        {
            if (!IsValidTile(x, y)) return null;
            return gridState[x, y];
        }
        public void SetVirusFlag(int x, int y, bool flagged)
        {
            if (!IsValidTile(x, y)) return;
            virusFlags[x, y] = flagged;
            OnGridStateChanged?.Invoke();
        }

        public bool IsFlaggedAsVirus(int x, int y)
        {
            return IsValidTile(x, y) && virusFlags[x, y];
        }
        public void PrecomputeEchoTiles()
        {
            echoRetentionTiles.Clear();

            // Only run echo retention if the Echo payload is active
            if (payloadManager != null && !payloadManager.IsPayloadActive(PayloadType.Echo))
            {
                Debug.Log("[Echo] Echo payload not active, skipping tile retention");
                return;
            }

            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    if (tileStates[x, y] == TileState.Revealed && UnityEngine.Random.value <= 0.3f)
                    {
                        echoRetentionTiles.Add(new Vector2Int(x, y));
                        Debug.Log($"[Echo] Will retain tile at ({x},{y})");
                    }
                }
            }
        }
        public void RestoreEchoTiles()
        {
            foreach (var pos in echoRetentionTiles)
            {
                if (IsValidTile(pos.x, pos.y))
                {
                    tileStates[pos.x, pos.y] = TileState.Revealed;
                    Debug.Log($"[Echo] Restored tile ({pos.x},{pos.y}) to revealed.");
                }
            }

            OnGridStateChanged?.Invoke();
        }
    }
}