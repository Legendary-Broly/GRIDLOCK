// IRoundService.cs
using NewGameplay.ScriptableObjects;

namespace NewGameplay.Interfaces
{
    public interface IRoundService
    {
        void ResetRound();
        event System.Action onRoundReset;
        //void TriggerRoundReset(); // Add this
        int GetGridSizeForRound(int round);
        int CurrentRound { get; }
        RoundConfigSO CurrentRoundConfig { get; }
    }
}
