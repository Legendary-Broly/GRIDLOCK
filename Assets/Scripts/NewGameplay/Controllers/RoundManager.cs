using UnityEngine;

public class RoundManager : MonoBehaviour
{
    private IRoundService roundService;
    private IProgressTrackerService progressService;

    public void Initialize(IRoundService roundService, IProgressTrackerService progressService)
    {
        this.roundService = roundService;
        this.progressService = progressService;
    }

    public void CheckRoundEnd()
    {
        if (progressService.CurrentScore >= roundService.CurrentThreshold)
        {
            Debug.Log($"[ROUND] Threshold {roundService.CurrentThreshold} reached. Resetting round.");
            roundService.ResetRound();
        }
    }
}
