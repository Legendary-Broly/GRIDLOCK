using System.Collections.Generic;

namespace NewGameplay.Interfaces
{
    public interface IInjectService
    {
        void SetFixedSymbols(List<string> symbols);
        void ResetForNewRound();
        List<string> GetCurrentSymbols();
        void InjectSymbol(string symbol, int x, int y);
        string SelectedSymbol { get; }
        void ClearSelectedSymbol();
        void ClearSymbolBank();
        void SetSelectedSymbol(int index);
    }
} 