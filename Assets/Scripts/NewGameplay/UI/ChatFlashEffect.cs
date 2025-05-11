using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class ChatFlashEffect : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.15f;
    [SerializeField] private int flashCount = 2;

    private Color originalColor;
    private float timer;
    private int state = -1; // -1 = idle, 0 = flashing in, 1 = flashing out
    private int flashIndex;

    public void BeginFlash()
    {
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        if (backgroundImage == null)
        {
            Debug.LogWarning("[ChatFlashEffect] No Image component found.");
            return;
        }

        StartCoroutine(DelayedFlash());
    }

    private IEnumerator DelayedFlash()
    {
        yield return null; // wait 1 frame to ensure layout/render is complete

        Color originalColor = backgroundImage.color;
        int flashes = 2;
        float flashDuration = 0.15f;
        Color flashColor = Color.red;

        for (int i = 0; i < flashes; i++)
        {
            backgroundImage.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            backgroundImage.color = originalColor;
            yield return new WaitForSeconds(flashDuration);
        }
    }

    private void Update()
    {
        if (state == -1) return;

        timer += Time.deltaTime;

        if (timer >= flashDuration)
        {
            timer = 0f;

            if (state == 0)
            {
                backgroundImage.color = flashColor;
                state = 1;
            }
            else
            {
                backgroundImage.color = originalColor;
                flashIndex++;
                if (flashIndex >= flashCount)
                {
                    state = -1;
                    backgroundImage.color = originalColor;
                    enabled = false;
                    return;
                }

                state = 0;
            }
        }
    }
}
