public interface IGridService
{
    void TryPlaceSymbol(int x, int y);
    void SpreadVirus();
    void SetSymbol(int x, int y, string symbol);
    string GetSymbolAt(int x, int y);
    bool IsTilePlayable(int x, int y);
    int GridSize { get; }
}