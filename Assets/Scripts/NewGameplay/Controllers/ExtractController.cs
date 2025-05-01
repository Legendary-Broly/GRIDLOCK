using UnityEngine;
using UnityEngine.UI;
using NewGameplay.Interfaces;
using NewGameplay.Services;

namespace NewGameplay.Controllers
{
    public class ExtractController : MonoBehaviour
    {
        private IProgressTrackerService progressService;
        private IDataFragmentService dataFragmentService;
        private RoundManager roundManager;


        [SerializeField] private Button extractButton;

        private void Start()
        {
            if (extractButton != null)
            {
                extractButton.interactable = false;
            }
        }

        private void Update()
        {
            if (progressService == null || dataFragmentService == null) return;

            bool canExtract = progressService.HasMetGoal() && !dataFragmentService.AnyRevealedFragmentsContainVirus();
            extractButton.interactable = canExtract;
        }

        public void Initialize(IProgressTrackerService progress, IDataFragmentService dataFragments, RoundManager roundManager)
        {
            this.progressService = progress;
            this.dataFragmentService = dataFragments;
            this.roundManager = roundManager;
        }

        public void OnExtractButtonClicked()
        {
            if (progressService.HasMetGoal() && !dataFragmentService.AnyRevealedFragmentsContainVirus())
            {
                Debug.Log("[ExtractController] Extract triggered, checking round end.");
                roundManager.CheckRoundEnd(); // triggers popup & flow
                extractButton.interactable = false;
            }
        }
    }
}