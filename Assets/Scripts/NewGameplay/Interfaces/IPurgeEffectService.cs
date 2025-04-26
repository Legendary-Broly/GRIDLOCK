using System;
using UnityEngine;

namespace NewGameplay.Interfaces
{
    public interface IPurgeEffectService
    {
        event Action OnPurgeProcessed;
        
        void ProcessPurges();
        bool HandlePurgeEffect(int x, int y);
        bool PurgeRowAndColumn(int purgeX, int purgeY);
        void EnableRowColumnPurge();
        void DisableRowColumnPurge();
        bool IsRowColumnPurgeEnabled();
    }
} 