using System;
using System.Collections.Generic;
using UnityEngine;
using NewGameplay.Interfaces;

namespace NewGameplay.Services
{
    public class PurgeEffectService : IPurgeEffectService
    {
        private readonly IGridStateService gridStateService;
        private readonly IEntropyService entropyService;
        private static bool rowColumnPurgeEnabled = false;
        private IMutationEffectService mutationEffectService;
        
        // Static instance to maintain reference across scene changes or reloads
        private static IMutationEffectService persistentMutationService;

        public event Action OnPurgeProcessed;

        public PurgeEffectService(IGridStateService gridStateService, IEntropyService entropyService)
        {
            this.gridStateService = gridStateService;
            this.entropyService = entropyService;
            
            // Check if we have a persistent reference to use
            if (persistentMutationService != null)
            {
                this.mutationEffectService = persistentMutationService;
                Debug.Log($"[PurgeEffectService] Using persistent MutationEffectService reference: {persistentMutationService.GetHashCode()}");
            }
        }

        public void SetMutationEffectService(IMutationEffectService mutationEffectService)
        {
            this.mutationEffectService = mutationEffectService;
            // Also store in static field to keep reference
            persistentMutationService = mutationEffectService;
            Debug.Log($"[PurgeEffectService] MutationEffectService set, instance: {mutationEffectService?.GetHashCode()}");
        }

        public void ProcessPurges()
        {
            Debug.Log($"[PurgeEffectService] ProcessPurges called, mutationEffectService is {(mutationEffectService == null ? "NULL" : "available")}");
            
            // Use both the instance and persistent reference as a failsafe
            IMutationEffectService serviceToUse = mutationEffectService ?? persistentMutationService;
            
            // Check if PurgePlus mutation is active using both flags
            bool isPurgePlusActive = serviceToUse?.IsMutationActive(MutationType.PurgePlus) ?? false;
            bool shouldUseRowColumnPurge = isPurgePlusActive || rowColumnPurgeEnabled;
            
            Debug.Log($"[Purge] Purge Plus mutation active: {isPurgePlusActive}, Row/Column purge enabled: {rowColumnPurgeEnabled}, shouldUseRowColumnPurge: {shouldUseRowColumnPurge}");
            
            List<Vector2Int> purgePositions = new();
            List<Vector2Int> adjacentToPurgePositions = new();

            // First collect all Δ positions
            for (int y = 0; y < gridStateService.GridSize; y++)
            {
                for (int x = 0; x < gridStateService.GridSize; x++)
                {
                    if (gridStateService.GetSymbolAt(x, y) == "∆")
                    {
                        Vector2Int pos = new Vector2Int(x, y);
                        purgePositions.Add(pos);
                        
                        // Check if this purge is adjacent to a virus
                        if (IsAdjacentToVirus(x, y))
                        {
                            adjacentToPurgePositions.Add(pos);
                        }
                    }
                }
            }

            Debug.Log($"[Purge] Found {purgePositions.Count} purge symbols, {adjacentToPurgePositions.Count} adjacent to viruses");

            // Only process purges that are adjacent to viruses
            foreach (var pos in adjacentToPurgePositions)
            {
                bool purgedAny = false;
                
                // Use both row/column and adjacent purge when PurgePlus is active
                if (shouldUseRowColumnPurge)
                {
                    Debug.Log($"[Purge] Using row/column purge at ({pos.x},{pos.y})");
                    purgedAny = PurgeRowAndColumn(pos.x, pos.y);
                }
                else
                {
                    Debug.Log($"[Purge] Using adjacent purge at ({pos.x},{pos.y})");
                    purgedAny = HandlePurgeEffect(pos.x, pos.y);
                }

                // Only consume the purge symbol if it was adjacent to a virus
                gridStateService.SetSymbol(pos.x, pos.y, null);
                gridStateService.SetTilePlayable(pos.x, pos.y, true);
            }

            OnPurgeProcessed?.Invoke();
        }

        public bool HandlePurgeEffect(int x, int y)
        {
            Debug.Log($"[∆] Starting purge effect at ({x},{y})");
            int purgedVirusCount = 0;
            Vector2Int[] directions = new[] {
                new Vector2Int(1, 0), new Vector2Int(-1, 0),
                new Vector2Int(0, 1), new Vector2Int(0, -1)
            };

            foreach (var dir in directions)
            {
                int nx = x + dir.x;
                int ny = y + dir.y;
                if (gridStateService.IsInBounds(nx, ny) && gridStateService.GetSymbolAt(nx, ny) == "X")
                {
                    Debug.Log($"[∆] Found virus at ({nx},{ny})");
                    gridStateService.SetSymbol(nx, ny, null);
                    gridStateService.SetTilePlayable(nx, ny, true);
                    purgedVirusCount++;
                }
            }

            Debug.Log($"[∆] Total viruses purged: {purgedVirusCount}");

            if (purgedVirusCount > 0 && entropyService != null)
            {
                float entropyReduction = 1 + purgedVirusCount;
                Debug.Log($"[∆] Attempting to reduce entropy by {entropyReduction}% (base 1 + {purgedVirusCount} viruses)");
                entropyService.ModifyEntropy(-entropyReduction);
            }

            return purgedVirusCount > 0;
        }

        public bool PurgeRowAndColumn(int purgeX, int purgeY)
        {
            int purgedVirusCount = 0;

            // Purge row
            for (int x = 0; x < gridStateService.GridSize; x++)
            {
                if (gridStateService.GetSymbolAt(x, purgeY) == "X")
                {
                    gridStateService.SetSymbol(x, purgeY, null);
                    gridStateService.SetTilePlayable(x, purgeY, true);
                    purgedVirusCount++;
                }
            }

            // Purge column
            for (int y = 0; y < gridStateService.GridSize; y++)
            {
                if (gridStateService.GetSymbolAt(purgeX, y) == "X")
                {
                    gridStateService.SetSymbol(purgeX, y, null);
                    gridStateService.SetTilePlayable(purgeX, y, true);
                    purgedVirusCount++;
                }
            }

            if (purgedVirusCount > 0 && entropyService != null)
            {
                float entropyReduction = 1 + purgedVirusCount;
                Debug.Log($"[∆+] Attempting to reduce entropy by {entropyReduction}% (base 1 + {purgedVirusCount} viruses)");
                entropyService.ModifyEntropy(-entropyReduction);
            }
            
            // Always return true so the purge symbol is consumed even if no viruses were purged
            return true;
        }

        public void EnableRowColumnPurge()
        {
            rowColumnPurgeEnabled = true;
            Debug.Log("[PurgeEffectService] Row/Column purge has been enabled");
            
            // Trigger immediate processing of any existing purge symbols
            ProcessPurges();
        }

        public void DisableRowColumnPurge()
        {
            rowColumnPurgeEnabled = false;
            Debug.Log("[PurgeEffectService] Row/Column purge has been disabled");
        }

        public bool IsRowColumnPurgeEnabled()
        {
            Debug.Log($"[PurgeEffectService] IsRowColumnPurgeEnabled called, returning: {rowColumnPurgeEnabled}");
            return rowColumnPurgeEnabled;
        }

        private bool IsAdjacentToVirus(int x, int y)
        {
            Vector2Int[] directions = new[] {
                new Vector2Int(1, 0), new Vector2Int(-1, 0),
                new Vector2Int(0, 1), new Vector2Int(0, -1)
            };

            foreach (var dir in directions)
            {
                int nx = x + dir.x;
                int ny = y + dir.y;
                if (gridStateService.IsInBounds(nx, ny) && gridStateService.GetSymbolAt(nx, ny) == "X")
                {
                    return true;
                }
            }
            return false;
        }
    }
} 