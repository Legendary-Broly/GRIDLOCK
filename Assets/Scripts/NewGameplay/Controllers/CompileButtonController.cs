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
        private IWeightedInjectService injectService;

        public void Initialize(ICodeShardTracker tracker, IWeightedInjectService injectService)
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

            injectService.UnlockNextHack();

            if (shardTracker.TrySpendShards())
            {
                feedbackText.text = "Hack unlocked!";
            }
            else
            {
                feedbackText.text = "No more hacks or insufficient shards.";
            }

            UpdateButtonState();
        }
    }
}
