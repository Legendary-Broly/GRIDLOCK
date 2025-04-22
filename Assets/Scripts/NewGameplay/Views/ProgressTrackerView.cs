using UnityEngine;
using TMPro;

public class ProgressTrackerView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI progressText;

    private IProgressTrackerService progress;

    public void Initialize(IProgressTrackerService progressService)
    {
        progress = progressService;
        Refresh();
    }

    public void Refresh()
    {
        int current = progress.CurrentScore;
        int target = progress.RoundTarget;

        progressText.text = $"Progress: {current}/{target}";

        // Optional visual indicator
        if (current >= target)
            progressText.color = Color.green;
        else
            progressText.color = Color.cyan;
    }
}
