using System;
using UnityEngine;

namespace NewGameplay.Interfaces
{
    public interface IDamageOverTimeService
    {
        event Action<float> OnDamageApplied;
        
        void AddDot(int damage, float duration);
        void TickDots();
        void ClearDots();
    }
}
