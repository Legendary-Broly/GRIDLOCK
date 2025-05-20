using UnityEngine;
using TMPro;
using UnityEngine.UI;
using NewGameplay.Interfaces;
using NewGameplay.ScriptableObjects;
using NewGameplay.Enums;
using System.Collections;

namespace NewGameplay.Views
{
    public class ChatLogView : MonoBehaviour
    {
        
        [SerializeField] private Transform contentRoot;
        [SerializeField] private GameObject chatEntryPrefab;
        [SerializeField] private ChatLogSettings settings;

        public void DisplayMessage(string message, ChatMessageType type, ChatDisplayMode mode = ChatDisplayMode.Instant, bool skipTyping = false)
        {
            GameObject entry = Instantiate(chatEntryPrefab, contentRoot);
            var text = entry.GetComponentInChildren<TextMeshProUGUI>();
            text.color = GetColorForType(type);

            if (skipTyping || mode == ChatDisplayMode.Instant)
            {
                text.text = message;

                // ⬇ Force layout and scroll
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)contentRoot);
                Canvas.ForceUpdateCanvases();
                ScrollToBottom();

                if (type == ChatMessageType.Virus)
                    entry.GetComponent<ChatFlashEffect>()?.BeginFlash();

                return;
            }

            // Extract prefix (e.g. <b>...</b>) from message
            string prefix = "";
            string body = message;

            int prefixEnd = message.IndexOf("</b>");
            if (prefixEnd != -1)
            {
                int length = prefixEnd + 4;
                prefix = message.Substring(0, length);
                body = message.Substring(length);
            }

            text.text = prefix;
            StartCoroutine(AnimateMessage(entry, text, body, type == ChatMessageType.Virus));
        }

        private IEnumerator AnimateMessage(GameObject entry, TextMeshProUGUI text, string messageBody, bool isVirus)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)contentRoot);
            Canvas.ForceUpdateCanvases();

            yield return new WaitForSeconds(0.2f);

            string fullText = text.text;

            for (int i = 0; i <= messageBody.Length; i++)
            {
                text.text = fullText + messageBody.Substring(0, i);
                yield return new WaitForSeconds(0.006f);
            }

            ScrollToBottom();

            if (isVirus)
            {
                entry.GetComponent<ChatFlashEffect>()?.BeginFlash();
            }
        }

        private void ScrollToBottom()
        {
            StartCoroutine(ScrollToBottomDelayed());
        }

        private IEnumerator ScrollToBottomDelayed()
        {
            // Wait one frame for Unity to finalize layout rebuild
            yield return null;

            var scrollRect = GetComponentInParent<ScrollRect>();
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }

        private Color GetColorForType(ChatMessageType type)
        {
            return type switch
            {
                ChatMessageType.Warning => settings.warningColor,
                ChatMessageType.Virus => settings.virusColor,
                ChatMessageType.Fragment => settings.fragmentColor,
                ChatMessageType.Pivot => settings.pivotColor,
                _ => settings.infoColor
            };
        }

    }
}
