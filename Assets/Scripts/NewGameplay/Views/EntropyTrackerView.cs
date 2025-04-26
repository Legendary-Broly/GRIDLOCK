using UnityEngine;
using TMPro;
using NewGameplay.Interfaces;

public class EntropyTrackerView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI entropyText;
    [SerializeField] private TMP_FontAsset firaCodeFont;

    private IEntropyService entropy;

    private void Start()
    {
        // Set FiraCode font
        if (firaCodeFont == null)
        {
            // Attempt to load the FiraCode font if not assigned in the inspector
            firaCodeFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/FiraCode-Regular SDF");
        }
        
        if (firaCodeFont != null)
        {
            entropyText.font = firaCodeFont;
        }
    }

    public void Initialize(IEntropyService entropyService)
    {
        entropy = entropyService;
        Refresh();
    }

    public void Refresh()
    {
        entropyText.text = $"ENTROPY: {entropy.EntropyPercent}%";

        // Optional: Add color thresholds
        if (entropy.EntropyPercent >= 75)
            entropyText.color = Color.red;
        else if (entropy.EntropyPercent >= 40)
            entropyText.color = Color.yellow;
        else
            entropyText.color = Color.cyan;
    }
}
