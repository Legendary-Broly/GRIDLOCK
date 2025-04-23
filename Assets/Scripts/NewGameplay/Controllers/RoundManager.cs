using UnityEngine;
using System.Collections;

public class RoundManager : MonoBehaviour
{
    private IRoundService roundService;
    private IProgressTrackerService progressService;
    private RoundPopupController popupController;
    private bool roundJustReset = false;

    public void Initialize(IRoundService roundService, IProgressTrackerService progressService, RoundPopupController popupController)
    {
        this.roundService = roundService;
        this.progressService = progressService;
        this.popupController = popupController;
        roundService.onRoundReset += () => CheckRoundEnd();  // Ensure it checks after reset
    }

    private bool roundInProgress = true;

    public void CheckRoundEnd()
    {
        if (progressService.HasMetGoal())
        {
            int currentRound = progressService.RoundTarget / 20; // Shows the current round
            popupController.ShowPopup(currentRound); // Show round transition popup

            // When the player continues:
            popupController.onContinue = () =>
            {
                // Increment threshold after the popup
                progressService.IncreaseThreshold();  // Increase threshold by 20 points

                // Reset the round's game state
                roundService.ResetRound();
            };
        }
    }
}

