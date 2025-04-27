using System;
using UnityEngine;

namespace NewGameplay.Interfaces
{
    public interface IVirusSpreadService
    {
        event Action OnVirusSpread;
        
        void SpreadVirus();
        void SetEntropyService(IEntropyService entropyService);
        void SetPurgeEffectService(IPurgeEffectService purgeEffectService);
    }
} 