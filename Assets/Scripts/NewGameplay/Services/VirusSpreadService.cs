using System;
using System.Collections.Generic;
using UnityEngine;
using NewGameplay.Interfaces;
using NewGameplay.Models;
using NewGameplay.Services;

namespace NewGameplay.Services
{
    public class VirusSpreadService : IVirusSpreadService
    {
        private readonly IGridStateService gridStateService;
        private IEntropyService entropyServiceInternal;
        private HashSet<Vector2Int> recentlyPurgedPositions = new HashSet<Vector2Int>();
        private int purgeActionCount = 0;
        public event Action OnVirusSpread;
        private GridService gridService;

        // Virus configuration
        public const string VIRUS_SYMBOL = "X";
        private static readonly Vector2Int[] SPREAD_DIRECTIONS = new[]
        {
            Vector2Int.up, Vector2Int.down,
            Vector2Int.left, Vector2Int.right
        };

        public VirusSpreadService(IGridStateService gridStateService)
        {
            this.gridStateService = gridStateService;
        }

        public void SetGridService(GridService gridService)
        {
            this.gridService = gridService;
        }

        public void TrySpreadFromExistingViruses()
        {
            List<Vector2Int> virusPositions = GetAllVirusPositions();
            List<Vector2Int> validSpreadTargets = new List<Vector2Int>();

            foreach (var virus in virusPositions)
            {
                foreach (var dir in SPREAD_DIRECTIONS)
                {
                    int nx = virus.x + dir.x;
                    int ny = virus.y + dir.y;

                    if (!gridStateService.IsInBounds(nx, ny)) continue;

                    string symbol = gridStateService.GetSymbolAt(nx, ny);

                    if (symbol == "∆:/run_PURGE.exe")
                    {
                        Debug.Log($"[VirusSpreadService] Virus at ({virus.x},{virus.y}) neutralized by purge at ({nx},{ny})");
                        gridStateService.SetSymbol(nx, ny, null); // Clear purge
                        gridStateService.SetTilePlayable(nx, ny, true);
                        OnVirusSpread?.Invoke(); // still invoke for UI refresh
                    }
                    else if (CanVirusSpreadTo(nx, ny))
                    {
                        validSpreadTargets.Add(new Vector2Int(nx, ny));
                    }
                }
            }

            if (validSpreadTargets.Count > 0)
            {
                var spreadPos = validSpreadTargets[UnityEngine.Random.Range(0, validSpreadTargets.Count)];
                gridStateService.SetSymbol(spreadPos.x, spreadPos.y, VIRUS_SYMBOL);
                gridStateService.SetTilePlayable(spreadPos.x, spreadPos.y, false);

                Debug.Log($"[VirusSpreadService] Virus spread to ({spreadPos.x},{spreadPos.y})");

                OnVirusSpread?.Invoke();
            }
            else
            {
                Debug.Log("[VirusSpreadService] No valid virus spread targets found.");
            }

            // Increment action count and clear purged positions after 2 actions
            purgeActionCount++;
            if (purgeActionCount >= 2)
            {
                recentlyPurgedPositions.Clear();
                purgeActionCount = 0;
            }
        }

        private List<Vector2Int> GetAllVirusPositions()
        {
            List<Vector2Int> virusPositions = new List<Vector2Int>();

            for (int x = 0; x < gridStateService.GridWidth; x++)
            {
                for (int y = 0; y < gridStateService.GridHeight; y++)
                {
                    if (gridStateService.GetSymbolAt(x, y) == VIRUS_SYMBOL)
                    {
                        virusPositions.Add(new Vector2Int(x, y));
                    }
                }
            }

            return virusPositions;
        }

        private bool CanVirusSpreadTo(int x, int y)
        {
            if (!gridStateService.IsInBounds(x, y)) return false;

            string symbol = gridStateService.GetSymbolAt(x, y);

            if (symbol == VIRUS_SYMBOL || symbol == "DF" || symbol == "∆:/run_PURGE.exe") return false;

            if (recentlyPurgedPositions.Contains(new Vector2Int(x, y)))
            {
                Debug.Log($"[VirusSpreadService] Position ({x},{y}) was recently purged, blocking spread");
                return false;
            }

            // Block virus spread to tiles that are marked as CanRevealTile for the player
            if (gridService != null && gridService.CanRevealTile(x, y))
            {
                Debug.Log($"[VirusSpreadService] Spread blocked at ({x},{y}) — tile is marked as revealable for player.");
                return false;
            }

            // Block tiles adjacent to last revealed tile
            if (gridService != null)
            {
                Vector2Int? last = gridService.GetLastRevealedTile();
                if (last.HasValue)
                {
                    foreach (var dir in SPREAD_DIRECTIONS)
                    {
                        if (last.Value + dir == new Vector2Int(x, y))
                        {
                            Debug.Log($"[VirusSpreadService] Spread blocked at ({x},{y}) — adjacent to last revealed tile.");
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public void SpreadVirus()
        {
            Debug.LogWarning("[VirusSpreadService] SpreadVirus() was called, but virus growth is now tied to symbol placement, not inject.");
        }

        public void SetEntropyService(IEntropyService entropyService)
        {
            this.entropyServiceInternal = entropyService;
        }

        private IPurgeEffectService purgeEffectService;

        public void SetPurgeEffectService(IPurgeEffectService purgeEffectService)
        {
            this.purgeEffectService = purgeEffectService;
        }

        public void SetSymbolPlacementService(ISymbolPlacementService symbolPlacementService)
        {
            throw new NotImplementedException();
        }

        public bool HasVirusAt(int x, int y)
        {
            return gridStateService.GetSymbolAt(x, y) == VIRUS_SYMBOL;
        }

        public void RemoveVirusAt(int x, int y)
        {
            gridStateService.SetSymbol(x, y, null);
            recentlyPurgedPositions.Add(new Vector2Int(x, y));
            purgeActionCount = 0; // Reset counter when new purge occurs
        }
    }
}
