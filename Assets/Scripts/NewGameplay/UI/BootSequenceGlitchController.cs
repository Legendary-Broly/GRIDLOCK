using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using URPGlitch;

public class BootSequenceGlitchController : MonoBehaviour
{
    [SerializeField] private Volume volume;
    [SerializeField] private float bootDuration = 11f; // Total duration until "I'll fix that" finishes typing

    private AnalogGlitchVolume analogGlitch;
    private DigitalGlitchVolume digitalGlitch;
    private float elapsedTime = 0f;
    private bool glitchActive = true;

    void Start()
    {
        if (volume.profile.TryGet(out analogGlitch) && volume.profile.TryGet(out digitalGlitch))
        {
            // Initialize glitch effects to 0
            analogGlitch.scanLineJitter.value = 0f;
            analogGlitch.verticalJump.value = 0f;
            analogGlitch.horizontalShake.value = 0f;
            analogGlitch.colorDrift.value = 0f;
            digitalGlitch.intensity.value = 0f;
        }
    }

    void Update()
    {
        if (!glitchActive) return;

        elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(elapsedTime / bootDuration); // Normalize progress from 0 to 1

        // Scale analog glitch effects
        analogGlitch.scanLineJitter.value = Mathf.Lerp(0.0f, 0.05f, t);
        analogGlitch.verticalJump.value = Mathf.Lerp(0.0f, 0.02f, t);
        analogGlitch.horizontalShake.value = Mathf.Lerp(0.0f, 0.0f, t);
        analogGlitch.colorDrift.value = Mathf.Lerp(0.0f, 0.15f, t);

        // Scale digital glitch (starts at 0 and stays 0)
        digitalGlitch.intensity.value = Mathf.Lerp(0f, 0.005f, t);

        // Stop updating after boot completes
        if (t >= 1f) glitchActive = false;
    }

    // Optional: Call this externally when "I'll fix that" finishes typing
    public void StopGlitch()
    {
        glitchActive = false;

        // Reset glitch effects to zero
        if (analogGlitch != null)
        {
            analogGlitch.scanLineJitter.value = 0f;
            analogGlitch.verticalJump.value = 0f;
            analogGlitch.horizontalShake.value = 0f;
            analogGlitch.colorDrift.value = 0f;
        }

        if (digitalGlitch != null)
        {
            digitalGlitch.intensity.value = 0f;
        }
    }
}
