using System.Collections.Generic;
using UnityEngine;
using NewGameplay.Interfaces;

namespace NewGameplay.Services
{
    public class DamageOverTimeService : IDamageOverTimeService
    {
        private readonly struct DotInstance
        {
            public readonly int RemainingTicks;
            public readonly float DamagePerTick;

            public DotInstance(int ticks, float damage)
            {
                RemainingTicks = ticks;
                DamagePerTick = damage;
            }

            public DotInstance Tick() => new DotInstance(RemainingTicks - 1, DamagePerTick);
        }

        private readonly List<DotInstance> activeDots = new();
        private readonly ISystemIntegrityService systemIntegrityService;

        public event System.Action<float> OnDamageApplied;

        public DamageOverTimeService(ISystemIntegrityService systemIntegrityService)
        {
            this.systemIntegrityService = systemIntegrityService;
        }

        public void AddDot(int damage, float duration)
        {
            int ticks = Mathf.CeilToInt(duration);
            float perTick = damage / (float)ticks;
            activeDots.Add(new DotInstance(ticks, perTick));
            Debug.Log($"[DOT] Virus revealed → added {ticks} damage ticks to DOT queue");
        }

        public void TickDots()
        {
            if (activeDots.Count == 0)
                return;

            List<DotInstance> next = new();

            foreach (var dot in activeDots)
            {
                systemIntegrityService.Decrease(dot.DamagePerTick);
                OnDamageApplied?.Invoke(dot.DamagePerTick);
                Debug.Log($"[DOT] Applied {dot.DamagePerTick:F2} damage — {dot.RemainingTicks - 1} ticks remain");

                if (dot.RemainingTicks > 1)
                    next.Add(dot.Tick());
            }

            activeDots.Clear();
            activeDots.AddRange(next);
        }

        public void ClearDots()
        {
            activeDots.Clear();
        }
    }
}
