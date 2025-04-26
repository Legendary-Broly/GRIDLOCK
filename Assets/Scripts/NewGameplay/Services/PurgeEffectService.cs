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
        private bool rowColumnPurgeEnabled = false;

        public event Action OnPurgeProcessed;

        public PurgeEffectService(IGridStateService gridStateService, IEntropyService entropyService)
        {
            this.gridStateService = gridStateService;
            this.entropyService = entropyService;
        }

        public void ProcessPurges()
        {
            Debug.Log($"[Purge] rowColumnPurgeEnabled state: {rowColumnPurgeEnabled}");
            List<Vector2Int> purgePositions = new();

            // First collect all Δ positions
            for (int y = 0; y < gridStateService.GridSize; y++)
            {
                for (int x = 0; x < gridStateService.GridSize; x++)
                {
                    if (gridStateService.GetSymbolAt(x, y) == "∆")
                    {
                        purgePositions.Add(new Vector2Int(x, y));
                    }
                }
            }

            Debug.Log($"[Purge] Found {purgePositions.Count} purge symbols to process");

            // Then process each Δ
            foreach (var pos in purgePositions)
            {
                bool purgedAny;
                if (rowColumnPurgeEnabled)
                {
                    Debug.Log($"[Purge] Using row/column purge at ({pos.x},{pos.y})");
                    purgedAny = PurgeRowAndColumn(pos.x, pos.y);
                }
                else
                {
                    Debug.Log($"[Purge] Using adjacent purge at ({pos.x},{pos.y})");
                    purgedAny = HandlePurgeEffect(pos.x, pos.y);
                }

                if (purgedAny)
                {
                    gridStateService.SetSymbol(pos.x, pos.y, null);
                    gridStateService.SetTilePlayable(pos.x, pos.y, true);
                }
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
            bool purgedAny = false;

            // Purge row
            for (int x = 0; x < gridStateService.GridSize; x++)
            {
                if (gridStateService.GetSymbolAt(x, purgeY) == "X")
                {
                    gridStateService.SetSymbol(x, purgeY, null);
                    gridStateService.SetTilePlayable(x, purgeY, true);
                    purgedAny = true;
                }
            }

            // Purge column
            for (int y = 0; y < gridStateService.GridSize; y++)
            {
                if (gridStateService.GetSymbolAt(purgeX, y) == "X")
                {
                    gridStateService.SetSymbol(purgeX, y, null);
                    gridStateService.SetTilePlayable(purgeX, y, true);
                    purgedAny = true;
                }
            }

            if (purgedAny && entropyService != null)
            {
                int purgedCount = 0;
                for (int x = 0; x < gridStateService.GridSize; x++)
                    if (gridStateService.GetSymbolAt(x, purgeY) == null) purgedCount++;
                for (int y = 0; y < gridStateService.GridSize; y++)
                    if (gridStateService.GetSymbolAt(purgeX, y) == null) purgedCount++;
                
                float entropyReduction = 1 + purgedCount;
                Debug.Log($"[∆+] Attempting to reduce entropy by {entropyReduction}% (base 1 + {purgedCount} viruses)");
                entropyService.ModifyEntropy(-entropyReduction);
            }

            return purgedAny;
        }

        public void EnableRowColumnPurge()
        {
            rowColumnPurgeEnabled = true;
        }

        public bool IsRowColumnPurgeEnabled()
        {
            return rowColumnPurgeEnabled;
        }
    }
} 