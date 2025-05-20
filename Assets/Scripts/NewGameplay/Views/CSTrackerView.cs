using UnityEngine;
using TMPro;
using NewGameplay.Interfaces;

namespace NewGameplay.Views
{
    public class CSTrackerView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI csText;
        private ICodeShardTracker codeShardTracker;

        public void Initialize(ICodeShardTracker tracker)
        {
            codeShardTracker = tracker;
            codeShardTracker.OnShardCountChanged += UpdateShardText;
            UpdateShardText();
        }

        private void UpdateShardText()
        {
            csText.text = $"$CRIPTS: [{codeShardTracker.CurrentShardCount}]";
        }

        private void OnDestroy()
        {
            if (codeShardTracker != null)
            {
                codeShardTracker.OnShardCountChanged -= UpdateShardText;
            }
        }
    }
}
