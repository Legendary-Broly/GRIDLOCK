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
        public static void ApplySymbolEffectAtPlacement(string symbol, int x, int y, IGridService gridService, ITileElementService tileElementService)
        {
            switch (symbol)
            {
                case "Ψ:/run_FORK.exe": // Scout
                    Debug.Log($"[SymbolEffectProcessor] Scout triggered — revealing nearby tiles.");
                    RevealAdjacentTiles(x, y, gridService);
                    break;

                case "Σ:/run_REPAIR.exe": // Stabilizer
                    Debug.Log($"[SymbolEffectProcessor] Stabilizer placed at ({x},{y}).");
                    break;

                case "∆:/run_PURGE.exe": // Purge
                    Debug.Log($"[SymbolEffectProcessor] Purge triggered at ({x},{y}).");
                    break;
            }
        }

        private static void RevealAdjacentTiles(int x, int y, IGridService gridService)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int newX = x + dx;
                    int newY = y + dy;

                    if (newX >= 0 && newX < gridService.GridWidth &&
                        newY >= 0 && newY < gridService.GridHeight)
                    {
                        gridService.RevealTile(newX, newY);
                    }
                }
            }
        }
    }
}
