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
            // Use SetSymbol directly but without the additional checks for special symbols
            gridService.SetSymbol(fragmentPosition.Value.x, fragmentPosition.Value.y, "DF");
            Debug.Log($"[DataFragmentService] Spawned fragment at position {fragmentPosition.Value}");
            
            // Mark the fragment tile as unplayable
            if (gridService is GridService gs && gs.TilePlayable[fragmentPosition.Value.x, fragmentPosition.Value.y])
            {
                Debug.Log($"[DataFragmentService] Making data fragment tile at {fragmentPosition.Value} unplayable");
                gs.TilePlayable[fragmentPosition.Value.x, fragmentPosition.Value.y] = false;
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
                if (gridService.IsInBounds(checkPos.x, checkPos.y) &&
                    string.IsNullOrEmpty(gridService.GetSymbolAt(checkPos.x, checkPos.y)))
                {
                    return false;  // Found an empty adjacent tile
                }
            }

            return true;  // All adjacent tiles are filled
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
