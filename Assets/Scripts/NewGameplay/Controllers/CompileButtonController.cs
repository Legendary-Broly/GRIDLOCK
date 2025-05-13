using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NewGameplay.Interfaces;

namespace NewGameplay.UI
{
    public class CompileButtonController : MonoBehaviour
    {
        [SerializeField] private Button compileButton;
        [SerializeField] private TextMeshProUGUI feedbackText;

        private ICodeShardTracker shardTracker;
        private IInjectService injectService;

        public void Initialize(ICodeShardTracker tracker, IInjectService injectService)
        {
            this.shardTracker = tracker;
            this.injectService = injectService;

            compileButton.onClick.AddListener(OnCompileClicked);
            shardTracker.OnShardCountChanged += UpdateButtonState;
            UpdateButtonState();
        }

        private void OnDestroy()
        {
            if (shardTracker != null)
                shardTracker.OnShardCountChanged -= UpdateButtonState;
        }

        private void UpdateButtonState()
        {
            compileButton.interactable = shardTracker.CurrentShardCount >= shardTracker.ShardsRequiredForNextHack;
        }

        private void OnCompileClicked()
        {
            if (!compileButton.interactable) return;

            if (shardTracker.TrySpendShards())
            {
                feedbackText.text = "Compiling...";
            }
            else
            {
                feedbackText.text = "No more hacks or insufficient shards.";
            }

            UpdateButtonState();
        }
    }
}