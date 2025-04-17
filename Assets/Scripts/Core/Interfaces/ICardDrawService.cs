using System.Collections.Generic;

public interface ICardDrawService
{
    SymbolCard DrawSymbolCard(bool applyDoom = true);
    SymbolDataSO GetWeightedSymbol();

}
