using UnityEngine;
using TMPro;
using NewGameplay.Interfaces;

namespace NewGameplay.Views
{
    public class CSTrackerView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI csText;
        private ICodeShardTrackerService codeShardTracker;

        public void Initialize(ICodeShardTrackerService tracker)
        {
            codeShardTracker = tracker;
            codeShardTracker.OnCodeShardsChanged += UpdateShardText;
            UpdateShardText(codeShardTracker.CurrentCodeShards);
        }

        private void UpdateShardText(int shardCount)
        {
            csText.text = "$CRIPTS: [" + shardCount + "]";
        }

        private void OnDestroy()
        {
            if (codeShardTracker != null)
            {
                codeShardTracker.OnCodeShardsChanged -= UpdateShardText;
            }
        }
    }
}
