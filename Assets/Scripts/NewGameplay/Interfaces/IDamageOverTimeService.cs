using UnityEngine;

namespace NewGameplay
{
    public interface IDamageOverTimeService
    {
        void AddDot(int ticks, float totalDamage);
        void TickDots();
        bool HasActiveDots();
    }
}
