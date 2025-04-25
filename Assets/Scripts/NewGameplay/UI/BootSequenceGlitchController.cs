using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using URPGlitch;

public class BootSequenceGlitchController : MonoBehaviour
{
    [SerializeField] private Volume volume;
    [SerializeField] private float bootDuration = 7f; // Reduced from 11f to 7f since we're starting at 4s
    [SerializeField] private float initialDelay = 4f; // Time to wait before starting glitch

    private AnalogGlitchVolume analogGlitch;
    private DigitalGlitchVolume digitalGlitch;
    private float elapsedTime = 0f;
    private bool glitchActive = false;
    private bool hasStarted = false;

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
        if (!hasStarted)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= initialDelay)
            {
                hasStarted = true;
                elapsedTime = 0f;
                glitchActive = true;
            }
            return;
        }

        if (!glitchActive) return;

        elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(elapsedTime / bootDuration); // Normalize progress from 0 to 1

        // Scale analog glitch effects
        analogGlitch.scanLineJitter.value = Mathf.Lerp(0.0f, 0.07f, t);
        analogGlitch.verticalJump.value = Mathf.Lerp(0.0f, 0.04f, t);
        analogGlitch.horizontalShake.value = Mathf.Lerp(0.0f, 0.05f, t);
        analogGlitch.colorDrift.value = Mathf.Lerp(0.0f, 0.2f, t);

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
