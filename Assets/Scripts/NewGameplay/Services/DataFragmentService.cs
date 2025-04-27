using UnityEngine;
using NewGameplay.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace NewGameplay.Services
{
    public class DataFragmentService : IDataFragmentService
    {
        private readonly IGridService gridService;
        private Vector2Int? fragmentPosition;
        private const string FRAGMENT_SYMBOL = "DF";

        public DataFragmentService(IGridService gridService)
        {
            this.gridService = gridService;
        }

        public void SpawnFragment()
        {
            if (fragmentPosition.HasValue) return;  // Prevent multiple spawns

            var emptyPositions = gridService.GetAllEmptyTilePositions();
            if (emptyPositions.Count == 0)
            {
                Debug.LogWarning("[DataFragmentService] Cannot spawn data fragment - no empty positions available");
                return;
            }

            fragmentPosition = emptyPositions[Random.Range(0, emptyPositions.Count)];
            
            // First set the symbol
            gridService.SetSymbol(fragmentPosition.Value.x, fragmentPosition.Value.y, FRAGMENT_SYMBOL);
            
            // Then make the tile unplayable
            gridService.SetTilePlayable(fragmentPosition.Value.x, fragmentPosition.Value.y, false);
            
            // Force a grid update to ensure the view is refreshed
            if (gridService is GridService gs)
            {
                Debug.Log($"[DataFragmentService] Triggering grid update for DF at {fragmentPosition.Value}");
                gs.TriggerGridUpdate();
            }
            
            Debug.Log($"[DataFragmentService] Spawned fragment at position {fragmentPosition.Value}");
            Debug.Log($"[DataFragmentService] SetSymbol at {fragmentPosition.Value} to {FRAGMENT_SYMBOL}. Current symbol: {gridService.GetSymbolAt(fragmentPosition.Value.x, fragmentPosition.Value.y)}");
            
            // Log the entire grid state for debugging
            for (int y = 0; y < gridService.GridHeight; y++)
            {
                string row = "";
                for (int x = 0; x < gridService.GridWidth; x++)
                {
                    row += gridService.GetSymbolAt(x, y) ?? " ";
                }
                Debug.Log($"[DataFragmentService] Grid row {y}: {row}");
            }
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
                    string.IsNullOrEmpty(gridService.GetSymbolAt(checkPos.x, checkPos.y)) ||
                    gridService.GetSymbolAt(checkPos.x, checkPos.y) == "X") // Exclude viruses
                {
                    return false;  // Found an empty or virus adjacent tile
                }
            }

            return true;  // All adjacent tiles are filled with non-virus symbols
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
