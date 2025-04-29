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
        public event Action OnVirusSpread;
        public VirusSpreadService(IGridStateService gridStateService)
        {
            this.gridStateService = gridStateService;
        }

        public void TrySpreadFromExistingViruses()
        {
            List<Vector2Int> virusPositions = GetAllVirusPositions();
            List<Vector2Int> validSpreadTargets = new List<Vector2Int>();

            foreach (var virus in virusPositions)
            {
                Vector2Int[] directions = new[]
                {
                    Vector2Int.up, Vector2Int.down,
                    Vector2Int.left, Vector2Int.right
                };

                foreach (var dir in directions)
                {
                    int nx = virus.x + dir.x;
                    int ny = virus.y + dir.y;

                    if (!gridStateService.IsInBounds(nx, ny)) continue;

                    string symbol = gridStateService.GetSymbolAt(nx, ny);

                    if (symbol == "∆")
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
                gridStateService.SetSymbol(spreadPos.x, spreadPos.y, "X");
                gridStateService.SetTilePlayable(spreadPos.x, spreadPos.y, false);

                Debug.Log($"[VirusSpreadService] Virus spread to ({spreadPos.x},{spreadPos.y})");

                OnVirusSpread?.Invoke();
            }
            else
            {
                Debug.Log("[VirusSpreadService] No valid virus spread targets found.");
            }
        }

        private List<Vector2Int> GetAllVirusPositions()
        {
            List<Vector2Int> virusPositions = new List<Vector2Int>();

            for (int x = 0; x < gridStateService.GridWidth; x++)
            {
                for (int y = 0; y < gridStateService.GridHeight; y++)
                {
                    if (gridStateService.GetSymbolAt(x, y) == "X")
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

            // Allow overwriting anything except Virus, DataFragment, or Purge
            return symbol != "X" && symbol != "DF" && symbol != "∆";
        }

        public void SpreadVirus()
        {
            Debug.LogWarning("[VirusSpreadService] SpreadVirus() was called, but virus growth is now tied to symbol placement, not inject.");
        }

        public void SetEntropyService(IEntropyService entropyService)
        {
            this.entropyServiceInternal = entropyService;
        }

        private IPurgeEffectService purgeEffectService; // add this field

        public void SetPurgeEffectService(IPurgeEffectService purgeEffectService)
        {
            this.purgeEffectService = purgeEffectService;
        }

        public void SetSymbolPlacementService(ISymbolPlacementService symbolPlacementService)
        {
            throw new NotImplementedException();
        }

    }
}
