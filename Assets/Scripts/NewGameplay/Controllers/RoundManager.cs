using UnityEngine;
using System.Collections;
using NewGameplay;
using NewGameplay.Interfaces;

public class RoundManager : MonoBehaviour
{
    private IRoundService roundService;
    private IProgressTrackerService progressService;
    private RoundPopupController popupController;
    private IDataFragmentService dataFragmentService;
    private bool isRoundTransitioning = false; // Flag to prevent redundant calls
    private bool hasThresholdIncreased = false; // Flag to track if threshold has been increased

    public void Initialize(IRoundService roundService, IProgressTrackerService progressService, RoundPopupController popupController, IDataFragmentService dataFragmentService)
    {
        this.roundService = roundService;
        this.progressService = progressService;
        this.popupController = popupController;
        this.dataFragmentService = dataFragmentService;
        roundService.onRoundReset += () => CheckRoundEnd(); 
    }
    
    public void CheckRoundEnd()
    {
        if (isRoundTransitioning || hasThresholdIncreased) return; // Prevent redundant calls

        Debug.Log($"[RoundManager] CheckRoundEnd - CurrentProgress: {progressService.CurrentProgress}, CurrentThreshold: {progressService.CurrentThreshold}");

        // Step 1: Check if progress met the threshold and spawn fragment if needed
        if (progressService.CurrentProgress >= progressService.CurrentThreshold && !dataFragmentService.IsFragmentPresent())
        {
            dataFragmentService.SpawnFragment();
            Debug.Log("[RoundManager] Data Fragment spawned on reaching 100% progress.");
            return; // Wait for player to surround and extract the fragment
        }

        // Step 2: If fragment is already present and surrounded, handle round transition
        if (dataFragmentService.IsFragmentPresent() && dataFragmentService.IsFragmentFullySurrounded())
        {
            isRoundTransitioning = true; // Set flag to true to block further calls

            int currentRound = progressService.RoundTarget / 20; // Shows the current round
            popupController.ShowPopup(currentRound); // Show round transition popup

            // Clear previous delegate to prevent multiple invocations
            popupController.onContinue = null;

            // When the player continues:
            popupController.onContinue = () =>
            {
                Debug.Log("Popup continue clicked. Increasing threshold."); // Debug log
                // Increment threshold after the popup
                if (!hasThresholdIncreased) // Double-check to ensure single increment
                {
                    progressService.IncreaseThreshold();  // Increase threshold by 20 points
                    hasThresholdIncreased = true; // Mark threshold as increased
                }

                Debug.Log("Threshold increased. Resetting round."); // Debug log
                // Reset the round's game state
                roundService.ResetRound();

                isRoundTransitioning = false; // Reset flag after round transition
                hasThresholdIncreased = false; // Reset threshold flag for the next round
            };
        }
    }
}

