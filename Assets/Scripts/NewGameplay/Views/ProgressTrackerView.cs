using UnityEngine;
using TMPro;
using System.Collections;
using NewGameplay.Utility;
using NewGameplay.Services;
using NewGameplay.Interfaces;

public class ProgressTrackerView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TMP_FontAsset firaCodeFont;

    private IProgressTrackerService progress;
    private IGridService gridService;

    private Vector3 originalPosition;
    private Coroutine jitterCoroutine;
    private const float MAX_JITTER = 3f;

    private void Start()
    {
        originalPosition = progressText.transform.localPosition;

        if (firaCodeFont == null)
            firaCodeFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/FiraCode-Regular SDF");

        if (firaCodeFont != null)
            progressText.font = firaCodeFont;

        //Refresh();
    }

    public void Initialize(IProgressTrackerService progressService, IGridService grid)
    {
        progress = progressService;
        gridService = grid;

        progress.OnProgressChanged += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        if (progress != null)
        {
            progress.OnProgressChanged -= Refresh;
        }
        
        if (jitterCoroutine != null)
        {
            StopCoroutine(jitterCoroutine);
            progressText.transform.localPosition = originalPosition;
        }
    }

    private void UpdateJitterEffect()
    {
        float intensity = Mathf.Clamp01((float)progress.FragmentsFound / progress.RequiredFragments);

        if (intensity <= 0f)
        {
            if (jitterCoroutine != null)
            {
                StopCoroutine(jitterCoroutine);
                jitterCoroutine = null;
                progressText.transform.localPosition = originalPosition;
            }
            return;
        }

        if (jitterCoroutine != null)
            StopCoroutine(jitterCoroutine);

        jitterCoroutine = StartCoroutine(JitterText(intensity));
    }

    private IEnumerator JitterText(float intensity)
    {
        while (true)
        {
            float xOffset = Random.Range(-1f, 1f) * MAX_JITTER * intensity;
            float yOffset = Random.Range(-1f, 1f) * MAX_JITTER * intensity;

            progressText.transform.localPosition = originalPosition + new Vector3(xOffset, yOffset, 0);
            yield return new WaitForSeconds(1f / 60f);
        }
    }

    public void Refresh()
    {
        progressText.text = $"FRAGMENTS:{progress.FragmentsFound}/{progress.RequiredFragments}";
        UpdateJitterEffect();
    }
}
