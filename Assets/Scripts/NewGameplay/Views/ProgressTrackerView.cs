using UnityEngine;
using TMPro;
using System.Collections;

public class ProgressTrackerView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI progressText;

    private IProgressTrackerService progress;
    public int ProgressThreshold { get; private set; }

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
    public void IncreaseThreshold()
    {
        // Increment threshold by 20
        ProgressThreshold += 20; 
    }
}
