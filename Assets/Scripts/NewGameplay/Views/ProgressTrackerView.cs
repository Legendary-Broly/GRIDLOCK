using UnityEngine;
using TMPro;
using System.Collections;
using NewGameplay.Utility;
using NewGameplay.Services;
using NewGameplay.Interfaces;
using System.Collections.Generic;

public class ProgressTrackerView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TMP_FontAsset firaCodeFont;

    private IProgressTrackerService progress;
    private IGridService gridService;
    private IEntropyService entropyService;
    public int ProgressThreshold { get; private set; }

    private readonly Color cyanColor = Color.cyan;
    private readonly Color greenColor = Color.green;
    private readonly Color yellowColor = Color.yellow;
    private readonly Color redColor = Color.red;

    private Vector3 originalPosition;
    private Coroutine jitterCoroutine;
    private const float MAX_JITTER = 3f; // Maximum jitter in pixels

    private void Start()
    {
        originalPosition = progressText.transform.localPosition;
        
        // Set FiraCode font
        if (firaCodeFont == null)
        {
            // Attempt to load the FiraCode font if not assigned in the inspector
            firaCodeFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/FiraCode-Regular SDF");
        }
        
        if (firaCodeFont != null)
        {
            progressText.font = firaCodeFont;
        }
    }

    public void Initialize(IProgressTrackerService progressService, IGridService grid, IEntropyService entropy)
    {
        progress = progressService;
        gridService = grid;
        entropyService = entropy;

        // Subscribe to grid updates
        if (grid is GridService gridServiceImpl)
        {
            gridServiceImpl.OnGridUpdated += UpdatePotentialScore;
        }

        Refresh();
        
        // Initial check for round end if already reached goal
        if (progress.HasMetGoal())
        {
            CheckRoundEndOnProgressUpdate();
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from grid updates
        if (gridService is GridService gridServiceImpl)
        {
            gridServiceImpl.OnGridUpdated -= UpdatePotentialScore;
        }

        // Stop jitter coroutine if it's running
        if (jitterCoroutine != null)
        {
            StopCoroutine(jitterCoroutine);
        }
    }

    private Color GetColorForPercentage(float percentage)
    {
        if (percentage <= 100f)
        {
            // Lerp from cyan to green (0% to 100%)
            return Color.Lerp(cyanColor, greenColor, percentage / 100f);
        }
        else
        {
            // Scale percentage to 0-1 range between 100% and 300%
            float t = (percentage - 100f) / 200f; // 200 is the range between 100% and 300%
            if (t <= 0.5f)
            {
                // First half: green to yellow (100% to 200%)
                return Color.Lerp(greenColor, yellowColor, t * 2f);
            }
            else
            {
                // Second half: yellow to red (200% to 300%)
                return Color.Lerp(yellowColor, redColor, (t - 0.5f) * 2f);
            }
        }
    }

    private float GetJitterIntensity(float percentage)
    {
        if (percentage < 1f)
            return 0f;
        
        // Scale from 0 to 1 between 1% and 300%
        return Mathf.Clamp01(percentage / 300f);
    }

    private IEnumerator JitterText(float intensity)
    {
        while (true)
        {
            float xOffset = Random.Range(-1f, 1f) * MAX_JITTER * intensity;
            float yOffset = Random.Range(-1f, 1f) * MAX_JITTER * intensity;
            
            progressText.transform.localPosition = originalPosition + new Vector3(xOffset, yOffset, 0);
            
            yield return new WaitForSeconds(1f/60f); // Update position 60 times per second
        }
    }

    private void UpdateJitterEffect(float percentage)
    {
        float intensity = GetJitterIntensity(percentage);
        
        // If no jitter needed and coroutine is running, stop it
        if (intensity == 0f)
        {
            if (jitterCoroutine != null)
            {
                StopCoroutine(jitterCoroutine);
                jitterCoroutine = null;
                progressText.transform.localPosition = originalPosition;
            }
            return;
        }

        // Start or update jitter effect
        if (jitterCoroutine == null)
        {
            jitterCoroutine = StartCoroutine(JitterText(intensity));
        }
        else
        {
            // Stop current coroutine and start a new one with updated intensity
            StopCoroutine(jitterCoroutine);
            jitterCoroutine = StartCoroutine(JitterText(intensity));
        }
    }

    private void UpdatePotentialScore()
    {
        int potentialScore = 0;

        for (int y = 0; y < gridService.GridHeight; y++)
        {
            for (int x = 0; x < gridService.GridWidth; x++)
            {
                string symbol = gridService.GetSymbolAt(x, y);
                if (string.IsNullOrEmpty(symbol)) continue;

                switch (symbol)
                {
                    case "Θ": // Loop
                    case "Σ": // Stabilizer
                    case "Ψ": // Scout
                        potentialScore += 1;
                        break;
                }
            }
        }

        float percentage = ((progress.CurrentScore + potentialScore) * 100f) / progress.RoundTarget;
        progressText.text = $"PROGRESS: {Mathf.RoundToInt(percentage)}%";
        progressText.color = GetColorForPercentage(percentage);

        UpdateJitterEffect(percentage);

        if (progress.HasMetGoal())
        {
            CheckRoundEndOnProgressUpdate();
        }
    }

    private void CheckRoundEndOnProgressUpdate()
    {
        // Find RoundManager and call CheckRoundEnd if available
        var roundManager = FindFirstObjectByType<RoundManager>();
        if (roundManager != null)
        {
            roundManager.CheckRoundEnd();
        }
    }

    public void Refresh()
    {
        UpdatePotentialScore();
    }

    public void IncreaseThreshold()
    {
        // Increment threshold by 20
        ProgressThreshold += 20; 
    }
}
