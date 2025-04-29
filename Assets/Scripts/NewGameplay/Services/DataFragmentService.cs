using UnityEngine;
using NewGameplay.Interfaces;
using System.Collections.Generic;
using System.Linq;
using NewGameplay.Models;
using NewGameplay.Enums;
using System;

namespace NewGameplay.Services
{
    public class DataFragmentService : IDataFragmentService
    {
        private readonly IGridService gridService;
        private Vector2Int? fragmentPosition;
        private const string FRAGMENT_SYMBOL = "DF";
        private readonly IGridStateService gridStateService;
        private GridViewNew gridView;
        public DataFragmentService(IGridService gridService)
        {
            this.gridService = gridService;
        }
        public void SetGridView(GridViewNew view)
        {
            gridView = view;
        }

        public void SpawnFragment()
        {
            if (fragmentPosition.HasValue)
            {
                Debug.Log("[DataFragmentService] Fragment already exists, skipping spawn.");
                return;
            }

            int width = gridService.GridWidth;
            int height = gridService.GridHeight;

            List<Vector2Int> validPositions = new List<Vector2Int>();

            for (int y = 1; y < height - 1; y++) // Avoid first/last row
            {
                for (int x = 1; x < width - 1; x++) // Avoid first/last column
                {
                    if (!gridService.IsTilePlayable(x, y)) continue;

                    string symbol = gridService.GetSymbolAt(x, y);

                    // Accept empty or virus tiles only
                    if (string.IsNullOrEmpty(symbol) || symbol == "X")
                    {
                        validPositions.Add(new Vector2Int(x, y));
                    }
                }
            }

            if (validPositions.Count == 0)
            {
                Debug.LogWarning("[DataFragmentService] No valid positions found for Data Fragment spawn.");
                return;
            }

            // Randomly select valid position
            Vector2Int selectedPosition = validPositions[UnityEngine.Random.Range(0, validPositions.Count)];
            fragmentPosition = selectedPosition;

            // Place symbol and mark the tile as revealed
            gridService.SetSymbol(selectedPosition.x, selectedPosition.y, "DF");
            gridService.SetTilePlayable(selectedPosition.x, selectedPosition.y, false);
            gridStateService.SetTileState(selectedPosition.x, selectedPosition.y, TileState.Revealed);

            Debug.Log($"[DataFragmentService] Spawned Data Fragment at {selectedPosition}");

            // Force immediate visual update
            gridView?.RefreshTileAt(selectedPosition.x, selectedPosition.y);

        }

        public bool IsFragmentFullySurrounded()
        {
            if (!fragmentPosition.HasValue) return false;

            Vector2Int pos = fragmentPosition.Value;
            Vector2Int[] directions = new Vector2Int[]
            {
                Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
            };

            foreach (var dir in directions)
            {
                Vector2Int checkPos = pos + dir;
                if (!gridService.IsInBounds(checkPos.x, checkPos.y) ||
                    !gridService.IsTileRevealed(checkPos.x, checkPos.y))
                {
                    return false;  // Adjacent tile is not revealed
                }
            }

            return true;  // All adjacent tiles are revealed
        }


        public Vector2Int? GetFragmentPosition()
        {
            return fragmentPosition;
        }

        public void ClearFragment()
        {
            if (fragmentPosition.HasValue)
            {
                Debug.Log($"[DataFragmentService] Clearing fragment at position {fragmentPosition.Value}");
                gridService.SetSymbol(fragmentPosition.Value.x, fragmentPosition.Value.y, "");
                fragmentPosition = null;
            }
            else
            {
                Debug.Log("[DataFragmentService] ClearFragment called but no fragment was present");
            }
        }

        public bool IsFragmentPresent()
        {
            return fragmentPosition.HasValue;
        }
    }
}
