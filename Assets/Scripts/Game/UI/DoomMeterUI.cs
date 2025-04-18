using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DoomMeterUI : MonoBehaviour
{
    [SerializeField] private Image meterFill;
    [SerializeField] private TextMeshProUGUI multiplierText;
    [SerializeField] private TextMeshProUGUI stageText;

    //private float maxFill = 1f; // Full bar

    public void UpdateDoomMeter(float doomChance, float doomMultiplier, int doomStage)
    {
        // Smooth fill based on % chance (0 to 1)
        float fillAmount = Mathf.Clamp01(doomChance);
        meterFill.fillAmount = fillAmount;

        // Optional: Color gradient (green → yellow → red)
        meterFill.color = Color.Lerp(Color.green, Color.red, fillAmount);
        multiplierText.text = $"{doomMultiplier:0.0}X";

        // New: Doom Stage indicator
        if (stageText != null)
        {
            stageText.text = $"STAGE {doomStage}";

            switch (doomStage)
            {
                case 1: stageText.color = Color.green; break;
                case 2: stageText.color = Color.yellow; break;
                case 3: stageText.color = Color.red; break;
                default: stageText.color = Color.white; break;
            }
        }
    }

}
