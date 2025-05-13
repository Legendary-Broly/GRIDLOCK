using System.Collections;
using UnityEngine;

namespace Michsky.DreamOS
{
    public class FrameSpinner : MonoBehaviour
    {
        [Header("Resources")]
        [SerializeField] private RectTransform targetTransform;

        [Header("Options")]
        [SerializeField] [Range(0.01f, 0.5f)] private float frameDelay = 0.1f;
        [SerializeField] [Range(1, 10)] private float moveBy = 10;
        [SerializeField] private float minPos = -150;
        [SerializeField] private float maxPos = 150;
        [SerializeField] private float startDelay = 1;

        void OnEnable()
        {
            targetTransform.anchoredPosition = new Vector2(minPos, targetTransform.anchoredPosition.y);
       
            if (startDelay > 0) { StartCoroutine("ProcessDelay"); }
            else { StartCoroutine("ProcessRect"); }
        }

        IEnumerator ProcessDelay()
        {
            yield return new WaitForSeconds(startDelay);
            StartCoroutine("ProcessRect");
        }

        IEnumerator ProcessRect()
        {
            yield return new WaitForSeconds(frameDelay);

            targetTransform.anchoredPosition = new Vector2(targetTransform.anchoredPosition.x + moveBy, targetTransform.anchoredPosition.y);
            if (targetTransform.anchoredPosition.x >= maxPos) { targetTransform.anchoredPosition = new Vector2(minPos, targetTransform.anchoredPosition.y); }

            StartCoroutine("ProcessRect");
        }
    }
}