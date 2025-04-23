using UnityEngine;
using System.Collections;

public class RoundManager : MonoBehaviour
{
    private IRoundService roundService;
    private IProgressTrackerService progressService;
    private RoundPopupController popupController;
    private bool isRoundTransitioning = false; // Flag to prevent redundant calls
    private bool hasThresholdIncreased = false; // Flag to track if threshold has been increased

    public void Initialize(IRoundService roundService, IProgressTrackerService progressService, RoundPopupController popupController)
    {
        this.roundService = roundService;
        this.progressService = progressService;
        this.popupController = popupController;
        roundService.onRoundReset += () => CheckRoundEnd();  // Ensure it checks after reset
    }

    public void CheckRoundEnd()
    {
        if (isRoundTransitioning || hasThresholdIncreased) return; // Prevent redundant calls

        if (progressService.HasMetGoal())
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

