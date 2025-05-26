using UnityEngine;
using SplitGrid.Interfaces;
using NewGameplay.Interfaces;
using NewGameplay.Strategies;


namespace SplitGrid.Services
{
    public class SplitVirusService : ISplitVirusService
    {
        private readonly ISplitGridService splitGridService;
        private readonly IVirusService virusServiceA;
        private readonly IVirusService virusServiceB;
        private readonly VirusSpawningStrategy virusSpawningStrategy;
        private readonly IChatLogService chatLogService;

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
        }

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
            int count = 0;
            int width = splitGridService.GetGridWidth(gridId);
            int height = splitGridService.GetGridHeight(gridId);

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    if (virusService.HasVirusAt(x, y))
                        count++;

            return count;
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