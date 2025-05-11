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

        public void DisplayMessage(string message, ChatMessageType type, ChatDisplayMode mode = ChatDisplayMode.Instant)
        {
            GameObject entry;

            if (mode == ChatDisplayMode.Typewriter)
            {
                entry = Instantiate(chatEntryPrefab, contentRoot);
                var text = entry.GetComponentInChildren<TextMeshProUGUI>();
                text.color = GetColorForType(type);
                text.text = "";

                bool isVirus = type == ChatMessageType.Virus;
                StartCoroutine(AnimateMessage(entry, text, message, isVirus));
            }
            else
            {
                entry = DisplayMessageInstant(message, type);

                if (type == ChatMessageType.Virus)
                {
                    var flash = entry.GetComponent<ChatFlashEffect>();
                    flash?.BeginFlash();
                }
            }
        }

        private GameObject DisplayMessageInstant(string message, ChatMessageType type)
        {
            var entry = Instantiate(chatEntryPrefab, contentRoot);
            var text = entry.GetComponentInChildren<TextMeshProUGUI>();
            text.text = message;
            text.color = GetColorForType(type);

            Canvas.ForceUpdateCanvases();

            // ⬇ Scroll to bottom
            ScrollToBottom();

            return entry;
        }

        private IEnumerator AnimateMessage(GameObject entry, TextMeshProUGUI text, string message, bool isVirus)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)contentRoot);
            Canvas.ForceUpdateCanvases();

            yield return new WaitForSeconds(0.3f); // optional startup delay

            for (int i = 0; i <= message.Length; i++)
            {
                text.text = message.Substring(0, i);
                yield return new WaitForSeconds(0.006f);
            }

            Canvas.ForceUpdateCanvases();

            // ⬇ Scroll to bottom
            ScrollToBottom();

            // 🔥 Trigger virus flash AFTER message is fully typed
            if (isVirus)
            {
                var flash = entry.GetComponent<ChatFlashEffect>();
                flash?.BeginFlash();
            }
        }

        private void ScrollToBottom()
        {
            var scrollRect = GetComponentInParent<ScrollRect>();
            if (scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
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
