using System.Collections.Generic;
using UnityEngine;
using NewGameplay.Interfaces;
using NewGameplay.Enums;
using NewGameplay.Models;
using System.Linq;
using NewGameplay.Services;

namespace NewGameplay.Utility
{
    public static class SymbolEffectProcessor
    {
        
        // Called at the end of extraction phase
        public static void ApplyPassiveEntropyPenalty(IGridService gridService, IEntropyService entropyService)
        {
            int virusCount = 0;
            for (int x = 0; x < gridService.GridWidth; x++)
            {
                for (int y = 0; y < gridService.GridHeight; y++)
                {
                    if (gridService.GetSymbolAt(x, y) == "X")
                        virusCount++;
                }
            }

            int penalty = virusCount / 5; // 1% entropy for every 5 viruses
            if (penalty > 0)
            {
                entropyService.Increase(penalty);
                Debug.Log($"[SymbolEffectProcessor] Applied passive entropy penalty: +{penalty}% due to {virusCount} viruses.");
            }
        }

        // New method: handle effects immediately on placement
        public static void ApplySymbolEffectAtPlacement(string symbol, int x, int y, IGridService gridService, IEntropyService entropyService, ITileElementService tileElementService)
        {
            switch (symbol)
            {

                case "Ψ:/run_FORK.exe": // Scout
                    Debug.Log($"[SymbolEffectProcessor] Scout triggered — revealing nearby tiles and increasing entropy.");
                    entropyService?.Increase(5);
                    RevealRandomHiddenTiles(gridService, x, y, 3, forceReveal: true);
                    break;

                case "Σ:/run_REPAIR.exe": // Stabilizer
                    Debug.Log($"[SymbolEffectProcessor] Stabilizer placed at ({x},{y}) — decreasing entropy.");
                    entropyService?.Decrease(5);
                    break;

                case "∆:/run_PURGE.exe": // Purge
                    // Purge behavior is handled separately inside placement logic (virus removal), no special side effect needed
                    break;
            }
        }

        private static void RevealRandomHiddenTiles(IGridService gridService, int centerX, int centerY, int count, bool forceReveal = false)
        {
            List<(int x, int y)> hiddenTiles = new();

            for (int j = 0; j < gridService.GridHeight; j++)
            {
                for (int i = 0; i < gridService.GridWidth; i++)
                {
                    if (gridService.GetTileState(i, j) == TileState.Hidden)
                    {
                        hiddenTiles.Add((i, j));
                    }
                }
            }

            hiddenTiles = hiddenTiles.OrderBy(_ => UnityEngine.Random.value).ToList();

            for (int i = 0; i < Mathf.Min(count, hiddenTiles.Count); i++)
            {
                var (rx, ry) = hiddenTiles[i];
                gridService.RevealTile(rx, ry, forceReveal: forceReveal);
                gridService.RefreshTile(rx, ry);
            }
        }
    }
}
