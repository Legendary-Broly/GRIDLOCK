// IRoundService.cs
public interface IRoundService
{
    int CurrentThreshold { get; }
    void ResetRound();
}