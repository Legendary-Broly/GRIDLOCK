using UnityEngine;

namespace SplitGrid.Interfaces
{
    public interface ISplitVirusService
    {
        // Virus Operations
        void SpawnViruses(int virusCountA, int virusCountB);
        bool HasVirusAt(GridID gridId, int x, int y);
        void RemoveVirus(GridID gridId, int x, int y);
        
        // Virus Counting
        int CountVirusesInColumn(GridID gridId, int col, int height);
        int CountVirusesInRow(GridID gridId, int row, int width);
        
        // Grid State
        bool AreGridsVirusFree();
        int GetTotalVirusCount(GridID gridId);
        
        // Virus Events
        void OnVirusRevealed(GridID gridId, int x, int y);
        void OnVirusRemoved(GridID gridId, int x, int y);
    }
} 