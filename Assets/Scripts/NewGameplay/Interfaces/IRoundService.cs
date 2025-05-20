// IRoundService.cs
public interface IRoundService
{
    void ResetRound();
    event System.Action onRoundReset;
    //void TriggerRoundReset(); // Add this
    int GetGridSizeForRound(int round);
    int CurrentRound { get; }
}
