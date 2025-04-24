// --- IInjectService.cs ---
public interface IInjectService
{
    string SelectedSymbol { get; }
    void InjectSymbols();
    void SelectSymbol(int index);
    void ClearSelectedSymbol(string symbol);
    void ClearSymbolBank();
    void ClearSelectedSymbol();
}