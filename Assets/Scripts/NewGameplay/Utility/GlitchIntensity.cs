using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using URPGlitch;
using NewGameplay.Services;

namespace NewGameplay.Utility
{
    public class EntropyGlitchController : MonoBehaviour
    {
        public Volume volume; // Assign Global Volume in inspector
        private AnalogGlitchVolume analogGlitch;
        private DigitalGlitchVolume digitalGlitch;

        private EntropyService entropyService;

        private void Start()
        {
            volume.profile.TryGet(out analogGlitch);
            volume.profile.TryGet(out digitalGlitch);
            
            // Get the EntropyService from the NewGameplayBootstrapper
            var bootstrapper = FindFirstObjectByType<NewGameplayBootstrapper>();
            if (bootstrapper != null)
            {
                entropyService = bootstrapper.ExposedEntropyService;
            }
            
            if (entropyService == null)
            {
                Debug.LogError("EntropyService not found! Make sure NewGameplayBootstrapper is properly initialized in the scene.");
                enabled = false; // Disable this component if we can't find the service
            }
        }

        private void Update()
        {
            if (entropyService == null) return;

            float normalizedEntropy = entropyService.EntropyPercent / 100f;

            // Scale glitch effects based on entropy (adjust multipliers as needed)
            if (analogGlitch != null)
            {
                analogGlitch.scanLineJitter.value = Mathf.Lerp(0.0f, 0.025f, normalizedEntropy);
                analogGlitch.verticalJump.value = Mathf.Lerp(0.0f, 0.01f, normalizedEntropy);
                analogGlitch.horizontalShake.value = Mathf.Lerp(0.0f, 0.0f, normalizedEntropy);
                analogGlitch.colorDrift.value = Mathf.Lerp(0.0f, 0.15f, normalizedEntropy);
            }

            if (normalizedEntropy >= 1f)
            {
                digitalGlitch.intensity.value = 0.009f;  // Activate at 100% entropy
            }
            else
            {
                digitalGlitch.intensity.value = 0f;  // Off otherwise
            }
        }
    }
}
