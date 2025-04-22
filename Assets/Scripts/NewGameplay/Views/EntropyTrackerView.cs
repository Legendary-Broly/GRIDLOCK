using UnityEngine;
using TMPro;

public class EntropyTrackerView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI entropyText;

    private IEntropyService entropy;

    public void Initialize(IEntropyService entropyService)
    {
        entropy = entropyService;
        Refresh();
    }

    public void Refresh()
    {
        entropyText.text = $"Entropy: {entropy.EntropyPercent}%";

        // Optional: Add color thresholds
        if (entropy.EntropyPercent >= 75)
            entropyText.color = Color.red;
        else if (entropy.EntropyPercent >= 40)
            entropyText.color = Color.yellow;
        else
            entropyText.color = Color.cyan;
    }
}
