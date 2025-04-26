using System;
using UnityEngine;

namespace NewGameplay.Interfaces
{
    public interface ISymbolPlacementService
    {
        event Action OnSymbolPlaced;
        
        void TryPlaceSymbol(int x, int y, string symbol);
        bool IsAdjacentToSymbol(int x, int y, string targetSymbol);
    }
} 