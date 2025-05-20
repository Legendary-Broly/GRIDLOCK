using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NewGameplay.Utility
{
    public class StartRunLoadingAnimator : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image fillBar;
        [SerializeField] private TextMeshProUGUI parseText;
        [SerializeField] private float totalDuration = 3.5f;

        [Header("Parse Strings")]
        [SerializeField] private List<string> parsingLines = new()
        {
            "boot.sys initialized...",
            "loading env.cfg",
            "parsing core.dll",
            "injecting entropy.dll",
            "validating protocols",
            "mounting slot system",
            "building application...",
            "cleaning temp...",
            "initializing gridlock.exe...",
            "establishing neural link...",
            "calibrating quantum core...",
            "syncing memory buffers...",
            "optimizing render pipeline...",
            "initializing AI subsystems...",
            "deploying security protocols...",
            "establishing network bridge...",
            "loading asset manifest...",
            "compiling shader cache...",
            "finalizing system checks...",
            "launching gridlock.exe...",
        };

        private void OnEnable()
        {
            StartCoroutine(AnimateLoading());
        }

        private IEnumerator AnimateLoading()
        {
            float currentFill = 0f;
            int parseIndex = 0;

            fillBar.fillAmount = 0f;

            while (currentFill < 1f)
            {
                // Chance to pause briefly
                if (Random.value < 0.2f)
                {
                    yield return new WaitForSeconds(Random.Range(0.3f, 0.6f));
                }

                // Random fill jump (erratic)
                float chunk = Random.Range(0.04f, 0.18f);
                float delay = Random.Range(0.05f, 0.25f);

                currentFill = Mathf.Clamp01(currentFill + chunk);
                fillBar.fillAmount = currentFill;

                // Update parse text if applicable
                if (parseIndex < parsingLines.Count)
                {
                    parseText.text = parsingLines[parseIndex];
                    parseIndex++;
                }

                yield return new WaitForSeconds(delay);
            }

            // Final state
            fillBar.fillAmount = 1f;
            parseText.text = "launching gridlock.exe...";
        }
    }
}
