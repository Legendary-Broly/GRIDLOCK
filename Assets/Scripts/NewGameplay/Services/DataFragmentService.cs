using UnityEngine;
using NewGameplay.Interfaces;
using System.Collections.Generic;
using System.Linq;
using NewGameplay.Models;
using NewGameplay.Enums;
using System;
using NewGameplay.Views;
using NewGameplay.Controllers;

namespace NewGameplay.Services
{
    public class DataFragmentService : IDataFragmentService
    {
        private readonly IGridService gridService;
        private readonly List<Vector2Int> fragmentPositions = new();

        private const string FRAGMENT_SYMBOL = "DATA";
        private GridViewNew gridView;
        private ITileElementService tileElementService;
        private PayloadManager payloadManager;


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

        }

        public void SpawnFragments(int count)
        {
            Debug.Log($"[DataFragmentService] Spawning {count} fragments");
            fragmentPositions.Clear();

            var allPositions = gridService.GetAllEmptyTilePositions();
            Debug.Log($"[DataFragmentService] Found {allPositions.Count} empty positions");

            if (tileElementService != null)
            {
                allPositions = allPositions
                    .Where(pos => tileElementService.GetElementAt(pos.x, pos.y) == TileElementType.Empty)
                    .ToList();
                Debug.Log($"[DataFragmentService] After filtering empty elements, {allPositions.Count} positions remain");
            }

            allPositions = allPositions.OrderBy(_ => UnityEngine.Random.value).ToList();

            for (int i = 0; i < count && allPositions.Count > 0; i++)
            {
                Vector2Int chosenPos;

                // First fragment: pick randomly
                if (i == 0 || !payloadManager.ShouldClusterDataFragments())
                {
                    chosenPos = allPositions[0];
                }
                else
                {
                    Vector2Int previous = fragmentPositions[i - 1];
                    List<Vector2Int> nearby = GetAdjacentUnoccupiedTiles(previous, allPositions);

                    bool cluster = UnityEngine.Random.value <= 0.5f;

                    if (cluster && nearby.Count > 0)
                    {
                        chosenPos = nearby[UnityEngine.Random.Range(0, nearby.Count)];
                    }
                    else
                    {
                        chosenPos = allPositions[0];
                    }
                }

                fragmentPositions.Add(chosenPos);
                gridService.SetSymbol(chosenPos.x, chosenPos.y, FRAGMENT_SYMBOL);
                RegisterFragmentAt(chosenPos.x, chosenPos.y);
                Debug.Log($"[DataFragmentService] Spawned fragment at ({chosenPos.x}, {chosenPos.y})");
                allPositions.Remove(chosenPos);
            }
            Debug.Log($"[DataFragmentService] Total fragments spawned: {fragmentPositions.Count}");
        }
        
        private List<Vector2Int> GetAdjacentUnoccupiedTiles(Vector2Int center, List<Vector2Int> validPool)
        {
            List<Vector2Int> result = new();

            Vector2Int[] directions = new Vector2Int[]
            {
                Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
            };

            foreach (var dir in directions)
            {
                Vector2Int neighbor = center + dir;
                if (validPool.Contains(neighbor))
                    result.Add(neighbor);
            }

            return result;
        }

        public bool AnyRevealedFragmentsContainVirus()
        {
            return fragmentPositions.Any(pos => gridService.GetSymbolAt(pos.x, pos.y) == "X" && gridService.IsTileRevealed(pos.x, pos.y));
        }

        public void SetPayloadManager(PayloadManager manager)
        {
            payloadManager = manager;
        }
        public void RegisterFragmentAt(int x, int y)
        {
            var pos = new Vector2Int(x, y);
            if (!fragmentPositions.Contains(pos))
            {
                fragmentPositions.Add(pos);
                Debug.Log($"[DataFragmentService] Registered fragment at ({x}, {y})");
            }
        }

        public bool IsFragmentAt(Vector2Int pos)
        {
            bool isFragment = fragmentPositions.Contains(pos);
            return isFragment;
        }
    }
}
