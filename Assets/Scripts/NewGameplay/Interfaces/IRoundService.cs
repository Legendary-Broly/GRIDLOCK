// IRoundService.cs
public interface IRoundService
{
    void ResetRound();
    event System.Action onRoundReset;
    void TriggerRoundReset(); // Add this
}
