using System;
using UnityEngine;
using NewGameplay.Interfaces;
using NewGameplay.Controllers;

namespace NewGameplay.Services
{
    public class SystemIntegrityService : ISystemIntegrityService
    {
        public event Action<float> OnIntegrityChanged;

        public float CurrentIntegrity { get; private set; } = 100f;
        private const float MAX_INTEGRITY = 100f;
        private const float MIN_INTEGRITY = 0f;
        private GameOverController gameOverController;

        public void Decrease(float amount)
        {
            CurrentIntegrity = Mathf.Clamp(CurrentIntegrity - amount, MIN_INTEGRITY, MAX_INTEGRITY);
            OnIntegrityChanged?.Invoke(CurrentIntegrity);

            if (Mathf.Approximately(CurrentIntegrity, 0f))
            {
                Debug.Log("[SystemIntegrityService] SYSTEM FAILURE: GRIDLOCK triggered.");
                gameOverController?.ShowGameOver(); // Trigger popup
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
        
        public void SetGameOverController(GameOverController controller)
        {
            this.gameOverController = controller;
        }
    }
}
