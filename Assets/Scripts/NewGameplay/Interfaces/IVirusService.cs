using System;
using UnityEngine;
using NewGameplay.Services;

namespace NewGameplay.Interfaces
{
    public interface IVirusService
    {
        event Action OnVirusCountChanged;
        
        bool HasVirusAt(int x, int y);
        void RemoveVirus(int x, int y);
        int CountVirusesInColumn(int col, int height);
        int CountVirusesInRow(int row, int width);
        void SpawnViruses(int count, int width, int height, Vector2Int? lastRevealedTile);
        void ClearViruses();
        int GetVirusCountInColumn(int column);
        int GetVirusCountInRow(int row);
        int GetTotalVirusCount();
    }
} 