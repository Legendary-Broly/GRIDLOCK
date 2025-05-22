using System;
using UnityEngine;
using NewGameplay.Services;

namespace NewGameplay.Interfaces
{
    public interface IVirusService
    {
        bool HasVirusAt(int x, int y);
        void RemoveVirus(int x, int y);
        int CountVirusesInColumn(int col, int height);
        int CountVirusesInRow(int row, int width);
    }
} 