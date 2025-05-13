using UnityEngine;

namespace NewGameplay.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Tutorial/Manual Section")]
    public class ManualSectionSO : ScriptableObject
    {
        public string title;
        [TextArea(5, 15)] public string body;
        public Sprite optionalImage;
    }
}
