using System;
using UnityEngine;
using NewGameplay.Services;

namespace NewGameplay.Interfaces
{
    public interface IVirusSpreadService
    {
        event Action OnVirusSpread;
        
        void SpreadVirus();
        void SetEntropyService(IEntropyService entropyService);
        void SetPurgeEffectService(IPurgeEffectService purgeEffectService);
        void SetSymbolPlacementService(ISymbolPlacementService symbolPlacementService);
        void TrySpreadFromExistingViruses();
        bool HasVirusAt(int x, int y);
        void RemoveVirusAt(int x, int y);
        void SetGridService(GridService gridService);
        int GetVirusGrowthRate();

    }
} 