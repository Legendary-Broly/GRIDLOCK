namespace NewGameplay.Interfaces
{
    public interface IWeightedInjectService
    {
        string SelectedSymbol { get; }
        void InjectSymbols();
        void SelectSymbol(int index);
        void ClearSelectedSymbol(string symbol);
        void ClearSymbolBank();
        void ClearSelectedSymbol();
        float GetSymbolWeight(string symbol);
        void UpdateWeights(float entropyPercent);
        string[] GetCurrentSymbols();
        event System.Action<string[]> OnSymbolsInjected;
        void UnlockNextHack();

    }
} 