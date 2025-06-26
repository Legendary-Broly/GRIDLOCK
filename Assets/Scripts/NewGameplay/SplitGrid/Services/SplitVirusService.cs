using UnityEngine;
using NewGameplay.SplitGrid.Interfaces;
using NewGameplay.SplitGrid.Services;
using NewGameplay.SplitGrid.Views;
using NewGameplay.SplitGrid.Data;
using NewGameplay.SplitGrid.Controllers;
using NewGameplay.Interfaces;
using NewGameplay.Strategies;
using NewGameplay.Services;
using NewGameplay.Controllers;
using System;

namespace NewGameplay.SplitGrid.Services
{
    public class SplitVirusService : ISplitVirusService
    {
        private readonly ISplitGridService splitGridService;
        private readonly IVirusService virusServiceA;
        private readonly IVirusService virusServiceB;
        private readonly VirusSpawningStrategy virusSpawningStrategy;
        private readonly IChatLogService chatLogService;

        public event Action OnVirusCountChanged;

        public SplitVirusService(
            ISplitGridService splitGridService,
            IVirusService virusServiceA,
            IVirusService virusServiceB,
            VirusSpawningStrategy virusSpawningStrategy,
            IChatLogService chatLogService)
        {
            this.splitGridService = splitGridService;
            this.virusServiceA = virusServiceA;
            this.virusServiceB = virusServiceB;
            this.virusSpawningStrategy = virusSpawningStrategy;
            this.chatLogService = chatLogService;

            // Subscribe to virus count changes
            virusServiceA.OnVirusCountChanged += () => OnVirusCountChanged?.Invoke();
            virusServiceB.OnVirusCountChanged += () => OnVirusCountChanged?.Invoke();
        }

        // IVirusService implementation
        public bool HasVirusAt(int x, int y) => HasVirusAt(GridID.GridA, x, y);
        public void RemoveVirus(int x, int y) => RemoveVirus(GridID.GridA, x, y);
        public int CountVirusesInColumn(int col, int height) => CountVirusesInColumn(GridID.GridA, col, height);
        public int CountVirusesInRow(int row, int width) => CountVirusesInRow(GridID.GridA, row, width);
        public void SpawnViruses(int count, int width, int height, Vector2Int? lastRevealedTile) => SpawnViruses(count, count);
        public void ClearViruses()
        {
            virusServiceA.ClearViruses();
            virusServiceB.ClearViruses();
        }
        public int GetVirusCountInColumn(int column) => virusServiceA.GetVirusCountInColumn(column);
        public int GetVirusCountInRow(int row) => virusServiceA.GetVirusCountInRow(row);
        public int GetTotalVirusCount() => GetTotalVirusCount(GridID.GridA) + GetTotalVirusCount(GridID.GridB);

        // ISplitVirusService implementation
        public void SpawnViruses(int virusCountA, int virusCountB)
        {
            // Spawn viruses for Grid A
            SpawnVirusesForGrid(GridID.GridA, virusCountA);

            // Spawn viruses for Grid B
            SpawnVirusesForGrid(GridID.GridB, virusCountB);
        }

        private void SpawnVirusesForGrid(GridID gridId, int virusCount)
        {
            var virusService = gridId == GridID.GridA ? virusServiceA : virusServiceB;
            int width = splitGridService.GetGridWidth(gridId);
            int height = splitGridService.GetGridHeight(gridId);

            // Get the last revealed tile to protect it
            Vector2Int? lastRevealedTile = splitGridService.GetLastRevealedTile(gridId);

            // Use the virus spawning strategy
            virusService.SpawnViruses(virusCount, width, height, lastRevealedTile);
        }

        public bool HasVirusAt(GridID gridId, int x, int y)
        {
            var virusService = gridId == GridID.GridA ? virusServiceA : virusServiceB;
            return virusService.HasVirusAt(x, y);
        }

        public void RemoveVirus(GridID gridId, int x, int y)
        {
            var virusService = gridId == GridID.GridA ? virusServiceA : virusServiceB;
            virusService.RemoveVirus(x, y);
        }

        public int CountVirusesInColumn(GridID gridId, int col, int height)
        {
            var virusService = gridId == GridID.GridA ? virusServiceA : virusServiceB;
            return virusService.CountVirusesInColumn(col, height);
        }

        public int CountVirusesInRow(GridID gridId, int row, int width)
        {
            var virusService = gridId == GridID.GridA ? virusServiceA : virusServiceB;
            return virusService.CountVirusesInRow(row, width);
        }

        public bool AreGridsVirusFree()
        {
            // Check Grid A
            for (int y = 0; y < splitGridService.GetGridHeight(GridID.GridA); y++)
                for (int x = 0; x < splitGridService.GetGridWidth(GridID.GridA); x++)
                    if (virusServiceA.HasVirusAt(x, y))
                        return false;

            // Check Grid B
            for (int y = 0; y < splitGridService.GetGridHeight(GridID.GridB); y++)
                for (int x = 0; x < splitGridService.GetGridWidth(GridID.GridB); x++)
                    if (virusServiceB.HasVirusAt(x, y))
                        return false;

            return true;
        }

        public int GetTotalVirusCount(GridID gridId)
        {
            var virusService = gridId == GridID.GridA ? virusServiceA : virusServiceB;
            return virusService.GetTotalVirusCount();
        }

        public void OnVirusRevealed(GridID gridId, int x, int y)
        {
            chatLogService?.LogVirusReveal();
        }

        public void OnVirusRemoved(GridID gridId, int x, int y)
        {
            // Handle virus removal if needed
        }
    }
} 