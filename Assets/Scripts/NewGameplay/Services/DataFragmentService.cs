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
        private List<Vector2Int> fragmentPositions = new();
        private const string FRAGMENT_SYMBOL = "DF";
        private GridViewNew gridView;
        private ITileElementService tileElementService;

        public DataFragmentService(IGridService gridService)
        {
            this.gridService = gridService;
        }

        public void SetGridView(GridViewNew view)
        {
            gridView = view;
        }

        public void SetTileElementService(ITileElementService service)
        {
            tileElementService = service;
            Debug.Log($"[DataFragmentService] TileElementService set: {(service != null ? "valid" : "null")}");
        }

        public void SpawnFragments(int count)
        {
            fragmentPositions.Clear();
            Debug.Log($"[DataFragmentService] Starting to spawn {count} data fragments");

            var positions = gridService.GetAllEmptyTilePositions();
            Debug.Log($"[DataFragmentService] Found {positions.Count} empty positions before filtering");
            
            // Filter positions to only include tiles with Empty element type and that are playable
            if (tileElementService != null)
            {
                positions = positions.Where(pos => 
                    tileElementService.GetElementAt(pos.x, pos.y) == TileElementType.Empty && 
                    gridService.IsTilePlayable(pos.x, pos.y)
                ).ToList();
                Debug.Log($"[DataFragmentService] After filtering for empty elements and playable tiles: {positions.Count} positions remain");
            }
            else
            {
                Debug.LogWarning("[DataFragmentService] TileElementService is null, skipping element type filtering");
            }
            
            positions = positions.OrderBy(_ => UnityEngine.Random.value).ToList();
            Debug.Log($"[DataFragmentService] Randomly ordered {positions.Count} valid positions");

            for (int i = 0; i < count && i < positions.Count; i++)
            {
                var pos = positions[i];
                fragmentPositions.Add(pos);
                gridService.SetSymbol(pos.x, pos.y, FRAGMENT_SYMBOL);
                gridService.SetTilePlayable(pos.x, pos.y, false);
                Debug.Log($"[DataFragmentService] Spawned fragment {i+1}/{count} at ({pos.x}, {pos.y}) - Tile playable: {gridService.IsTilePlayable(pos.x, pos.y)}, Element type: {tileElementService?.GetElementAt(pos.x, pos.y)}");
            }

            Debug.Log($"[DataFragmentService] Completed spawning {fragmentPositions.Count} fragments. Final fragment positions: {string.Join(", ", fragmentPositions.Select(p => $"({p.x},{p.y})"))}");
        }

        public int GetRevealedFragmentCount()
        {
            return fragmentPositions.Count(pos => gridService.IsTileRevealed(pos.x, pos.y));
        }

        public bool AnyRevealedFragmentsContainVirus()
        {
            return fragmentPositions.Any(pos => gridService.GetSymbolAt(pos.x, pos.y) == "X" && gridService.IsTileRevealed(pos.x, pos.y));
        }
    }
}
