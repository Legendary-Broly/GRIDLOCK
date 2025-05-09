using System;
using UnityEngine;
using NewGameplay.Interfaces;


namespace NewGameplay.Services
{
    public class SystemIntegrityService : ISystemIntegrityService
    {
        public event Action<float> OnIntegrityChanged;

        public float CurrentIntegrity { get; private set; } = 100f;
        private const float MAX_INTEGRITY = 100f;
        private const float MIN_INTEGRITY = 0f;

        public void Decrease(float amount)
        {
            CurrentIntegrity = Mathf.Clamp(CurrentIntegrity - amount, MIN_INTEGRITY, MAX_INTEGRITY);
            OnIntegrityChanged?.Invoke(CurrentIntegrity);

            if (Mathf.Approximately(CurrentIntegrity, 0f))
            {
                Debug.Log("[SystemIntegrityService] SYSTEM FAILURE: GRIDLOCK triggered.");
                // TODO: Trigger GRIDLOCK event / game over logic
            }
        }

        public void Increase(float amount)
        {
            CurrentIntegrity = Mathf.Clamp(CurrentIntegrity + amount, MIN_INTEGRITY, MAX_INTEGRITY);
            OnIntegrityChanged?.Invoke(CurrentIntegrity);
        }

        public void SetIntegrity(float amount)
        {
            CurrentIntegrity = Mathf.Clamp(amount, MIN_INTEGRITY, MAX_INTEGRITY);
            OnIntegrityChanged?.Invoke(CurrentIntegrity);
        }
    }
}
