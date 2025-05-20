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

        [Header("Normal Scaling Values")]
        [SerializeField] private float maxScanLineJitter = 0.05f;
        [SerializeField] private float maxVerticalJump = 0.03f;
        [SerializeField] private float maxHorizontalShake = 0.02f;
        [SerializeField] private float maxColorDrift = 0.07f;
        [SerializeField] private float maxDigitalIntensity = 0.001f;

        [Header("Zero Integrity Values")]
        [SerializeField] private float zeroScanLineJitter = 0.15f;
        [SerializeField] private float zeroVerticalJump = 0.1f;
        [SerializeField] private float zeroHorizontalShake = 0.08f;
        [SerializeField] private float zeroColorDrift = 0.2f;
        [SerializeField] private float zeroDigitalIntensity = 0.005f;

        [Header("Fluctuation Settings")]
        [SerializeField] private float minFluctuationSpeed = 0.5f;
        [SerializeField] private float maxFluctuationSpeed = 3f;
        [SerializeField] private float fluctuationIntensity = 0.5f;
        [SerializeField] private float randomChangeInterval = 2f;

        private float[] timeOffsets;
        private float[] currentSpeeds;
        private float[] nextSpeedChangeTimes;
        private float[] currentIntensities;

        private void Awake()
        {
            // Get volume components
            if (volume != null)
            {
                volume.profile.TryGet(out analogGlitch);
                volume.profile.TryGet(out digitalGlitch);
            }
            else
            {
                Debug.LogError("[GlitchIntensity] Volume component not assigned!");
                enabled = false;
                return;
            }

            // Initialize arrays for each effect
            timeOffsets = new float[5];
            currentSpeeds = new float[5];
            nextSpeedChangeTimes = new float[5];
            currentIntensities = new float[5];

            // Set initial random values
            for (int i = 0; i < 5; i++)
            {
                timeOffsets[i] = Random.Range(0f, 100f);
                currentSpeeds[i] = Random.Range(minFluctuationSpeed, maxFluctuationSpeed);
                nextSpeedChangeTimes[i] = Time.time + Random.Range(0f, randomChangeInterval);
                currentIntensities[i] = Random.Range(0f, fluctuationIntensity);
            }
        }

        private void Start()
        {
            // Get bootstrapper reference
            NewGameplayBootstrapper bootstrapper = bootstrapperReference;
            if (bootstrapper == null)
            {
                bootstrapper = Object.FindFirstObjectByType<NewGameplayBootstrapper>();
            }

            if (bootstrapper != null)
            {
                integrityService = bootstrapper.ExposedSystemIntegrityService;
                if (integrityService != null)
                {
                    Debug.Log("[GlitchIntensity] Successfully connected to SystemIntegrityService");
                }
            }

            if (integrityService == null)
            {
                Debug.LogError("[GlitchIntensity] SystemIntegrityService not found! Make sure NewGameplayBootstrapper is properly initialized in the scene.");
                enabled = false;
            }
        }

        private void UpdateFluctuationParameters(int index, float currentTime)
        {
            if (currentTime >= nextSpeedChangeTimes[index])
            {
                // Randomly decide if this effect should be active
                if (Random.value < 0.7f) // 70% chance to be active
                {
                    currentSpeeds[index] = Random.Range(minFluctuationSpeed, maxFluctuationSpeed);
                    currentIntensities[index] = Random.Range(0f, fluctuationIntensity);
                }
                else
                {
                    currentSpeeds[index] = 0f; // Effect temporarily stops fluctuating
                    currentIntensities[index] = 0f;
                }
                
                nextSpeedChangeTimes[index] = currentTime + Random.Range(0.5f, randomChangeInterval);
            }
        }

        private float GetFluctuatedValue(float baseValue, float time, int index)
        {
            UpdateFluctuationParameters(index, time);
            
            if (currentSpeeds[index] == 0f)
                return baseValue;

            float fluctuation = Mathf.Sin((time + timeOffsets[index]) * currentSpeeds[index]) * currentIntensities[index];
            return baseValue * (1f + fluctuation);
        }

        private void Update()
        {
            if (integrityService == null) return;

            float currentIntegrity = integrityService.CurrentIntegrity;
            float time = Time.time;
            
            // Check for zero integrity case
            if (Mathf.Approximately(currentIntegrity, 0f))
            {
                if (analogGlitch != null)
                {
                    analogGlitch.scanLineJitter.value = zeroScanLineJitter;
                    analogGlitch.verticalJump.value = zeroVerticalJump;
                    analogGlitch.horizontalShake.value = zeroHorizontalShake;
                    analogGlitch.colorDrift.value = zeroColorDrift;
                }

                if (digitalGlitch != null)
                {
                    digitalGlitch.intensity.value = zeroDigitalIntensity;
                }
                return;
            }

            // Normal scaling case
            float damagePercent = 1f - (currentIntegrity / 100f);

            if (analogGlitch != null)
            {
                float targetScanLine = Mathf.Lerp(0.0f, maxScanLineJitter, damagePercent);
                float targetVertical = Mathf.Lerp(0.0f, maxVerticalJump, damagePercent);
                float targetHorizontal = Mathf.Lerp(0.0f, maxHorizontalShake, damagePercent);
                float targetColor = Mathf.Lerp(0.0f, maxColorDrift, damagePercent);

                analogGlitch.scanLineJitter.value = GetFluctuatedValue(targetScanLine, time, 0);
                analogGlitch.verticalJump.value = GetFluctuatedValue(targetVertical, time, 1);
                analogGlitch.horizontalShake.value = GetFluctuatedValue(targetHorizontal, time, 2);
                analogGlitch.colorDrift.value = GetFluctuatedValue(targetColor, time, 3);
            }

            if (digitalGlitch != null)
            {
                float targetDigital = Mathf.Lerp(0.0f, maxDigitalIntensity, damagePercent);
                digitalGlitch.intensity.value = GetFluctuatedValue(targetDigital, time, 4);
            }
        }
    }
}
