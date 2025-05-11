using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using URPGlitch;
using NewGameplay.Services;
using NewGameplay;
using System.Collections;
using System.Linq;
using NewGameplay.Interfaces;

namespace NewGameplay.Utility
{
    public class GlitchIntensity : MonoBehaviour
    {
        public Volume volume; // Assign Global Volume in inspector
        private AnalogGlitchVolume analogGlitch;
        private DigitalGlitchVolume digitalGlitch;

        private ISystemIntegrityService integrityService;
        
        // Optional: Allow direct reference to bootstrapper through inspector
        [SerializeField] private NewGameplayBootstrapper bootstrapperReference;

        private IEnumerator Start()
        {
            yield return null; // Wait 1 frame to ensure Bootstrapper fully initialized

            volume.profile.TryGet(out analogGlitch);
            volume.profile.TryGet(out digitalGlitch);

            NewGameplayBootstrapper bootstrapper = bootstrapperReference;

            if (bootstrapper == null)
            {
                bootstrapper = Object.FindFirstObjectByType<NewGameplayBootstrapper>();
                Debug.Log("[GlitchIntensity] Looking up NewGameplayBootstrapper using FindFirstObjectByType");
            }
            else
            {
                Debug.Log("[GlitchIntensity] Using directly assigned bootstrapper reference");
            }

            if (bootstrapper != null)
            {
                Debug.Log("[GlitchIntensity] Found NewGameplayBootstrapper instance");
                integrityService = bootstrapper.GetComponent<ISystemIntegrityService>();
                if (integrityService != null)
                {
                    Debug.Log("[GlitchIntensity] Successfully connected to SystemIntegrityService");
                }
                else
                {
                    Debug.LogError("[GlitchIntensity] ExposedSystemIntegrityService is null in NewGameplayBootstrapper");
                }
            }
            else
            {
                Debug.LogError("[GlitchIntensity] NewGameplayBootstrapper not found in the scene");
            }

            if (integrityService == null)
            {
                Debug.LogError("SystemIntegrityService not found! Make sure NewGameplayBootstrapper is properly initialized in the scene.");
                enabled = false;
            }
        }

        private void Update()
        {
            if (integrityService == null) return;

            float normalizedIntegrity = integrityService.CurrentIntegrity / 100f;

            // Scale glitch effects based on entropy (adjust multipliers as needed)
            if (analogGlitch != null)
            {
                analogGlitch.scanLineJitter.value = Mathf.Lerp(0.0f, 0.025f, normalizedIntegrity);
                analogGlitch.verticalJump.value = Mathf.Lerp(0.0f, 0.01f, normalizedIntegrity);
                analogGlitch.horizontalShake.value = Mathf.Lerp(0.0f, 0.0f, normalizedIntegrity);
                analogGlitch.colorDrift.value = Mathf.Lerp(0.0f, 0.15f, normalizedIntegrity);
            }

            if (normalizedIntegrity >= 1f)
            {
                digitalGlitch.intensity.value = 0.009f;  // Activate at 100% integrity
            }
            else
            {
                digitalGlitch.intensity.value = 0f;  // Off otherwise
            }
        }
    }
}
