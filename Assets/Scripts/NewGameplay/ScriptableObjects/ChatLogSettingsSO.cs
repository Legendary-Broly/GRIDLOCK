using UnityEngine;

namespace NewGameplay.ScriptableObjects
{
    public enum ChatMessageType { Info, Warning, Virus, Fragment, Pivot }

    [CreateAssetMenu(fileName = "ChatLogSettings", menuName = "Config/ChatLogSettings")]
    public class ChatLogSettings : ScriptableObject
    {
        public Color infoColor;
        public Color warningColor;
        public Color virusColor;
        public Color fragmentColor;
        public Color pivotColor;
    }
}
