using System;
using System.Collections.Generic;
using UnityEngine;
using NewGameplay.Interfaces;
using NewGameplay.Models;
using NewGameplay.Services;

namespace NewGameplay.Services
{
    public class VirusService : IVirusService
    {
        private IGridStateService gridStateService;
        private IGridService gridService;

        // Virus configuration
        public const string VIRUS_SYMBOL = "X";

        public VirusService(IGridStateService gridStateService)
        {
            this.gridStateService = gridStateService;
        }

        public bool HasVirusAt(int x, int y)
        {
            var symbol = gridStateService.GetSymbolAt(x, y);
            return symbol == VIRUS_SYMBOL;
        }

        public void RemoveVirus(int x, int y)
        {
            if (HasVirusAt(x, y))
            {
                gridService.SetSymbol(x, y, "");
            }
        }

        public void SetGridService(IGridService gridService)
        {
            this.gridService = gridService;
        }

        public void SpawnViruses(int virusCount, int width, int height, Vector2Int? protectedTile = null)
        {
            var placed = 0;
            var attempted = 0;
            var random = new System.Random();

            while (placed < virusCount && attempted < virusCount * 10)
            {
                int x = random.Next(0, width);
                int y = random.Next(0, height);
                attempted++;

                // Skip if this is the protected tile
                if (protectedTile.HasValue && x == protectedTile.Value.x && y == protectedTile.Value.y)
                {
                    continue;
                }

                var symbol = gridStateService.GetSymbolAt(x, y);
                if (string.IsNullOrEmpty(symbol))
                {
                    gridService.SetSymbol(x, y, VIRUS_SYMBOL);
                    placed++;
                }
            }

            Debug.Log($"[VirusService] Spawned {placed} viruses after {attempted} attempts.");
        }
    }
}
