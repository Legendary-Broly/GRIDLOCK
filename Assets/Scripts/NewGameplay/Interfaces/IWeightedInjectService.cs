namespace NewGameplay.Interfaces
{
    public interface IWeightedInjectService : IInjectService
    {
        float GetSymbolWeight(string symbol);
        void UpdateWeights(float entropyPercent);
        string[] GetCurrentSymbols();
        event System.Action<string[]> OnSymbolsInjected;
    }
} 