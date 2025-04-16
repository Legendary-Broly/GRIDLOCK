using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DoomMeterUI : MonoBehaviour
{
    [SerializeField] private Image meterFill;
    [SerializeField] private TextMeshProUGUI multiplierText;

    private float fillAmount = 0f;
    private float maxFill = 1f; // Full bar

    public void UpdateDoomMeter(float doomChance, float doomMultiplier)
    {
        // Smooth fill based on % chance (0 to 1)
        fillAmount = Mathf.Clamp01(doomChance);

        meterFill.fillAmount = fillAmount;

        // Optional: Color gradient (green → yellow → red)
        meterFill.color = Color.Lerp(Color.green, Color.red, fillAmount);

        multiplierText.text = $"{doomMultiplier:0.0}X";
    }
}
