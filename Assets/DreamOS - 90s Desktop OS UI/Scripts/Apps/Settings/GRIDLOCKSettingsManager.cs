using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Diagnostics;

public class SettingsController : MonoBehaviour
{
    [Header("Video Settings")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown fullscreenDropdown;
    public Slider brightnessSlider;
    public Toggle lensDistortionToggle;
    public Toggle chromaticAberrationToggle;
    public Toggle filmGrainToggle;
    public Toggle vignetteToggle;
    public Slider glitchSlider;

    [Header("Audio Settings")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("Accessibility")]
    public TMP_Dropdown colorblindDropdown;

    [Header("Data")]
    public Button resetProgressButton;
    public Button viewSaveLocationButton;

    private void Start()
    {
        ApplySavedSettings();
    }

    private void ApplySavedSettings()
    {
        // Load from PlayerPrefs or custom save system
    }

    public void OnResetAllProgress()
    {
        // Confirm before wipe
        UnityEngine.Debug.Log("Resetting all progress...");
        // Implement wipe
    }

    public void OnViewSaveLocation()
    {
#if UNITY_STANDALONE_WIN
        string path = Application.persistentDataPath;
        Process.Start("explorer.exe", path.Replace("/", "\\"));
#endif
    }
}
