using System.Collections.Generic;
using NewGameplay.Interfaces;  
using NewGameplay.Services;
using NewGameplay.Models;

namespace NewGameplay.Services
{
    public class InjectService : IInjectService
    {
        private List<string> currentSymbols = new List<string>();
        private int selectedSymbolIndex = -1;
        private IGridService gridService;
        private List<string> fixedSymbols;
        private string selectedSymbol;

        public string SelectedSymbol => selectedSymbol;

        public void SetFixedSymbols(List<string> symbols)
        {
            fixedSymbols = new List<string>(symbols);
            ResetForNewRound();
        }

        public void ResetForNewRound()
        {
            currentSymbols = new List<string>(fixedSymbols);
            selectedSymbol = null;
        }

        public List<string> GetCurrentSymbols()
        {
            return new List<string>(currentSymbols);
        }

        public void SelectSymbol(int index)
        {
            if (index >= 0 && index < currentSymbols.Count)
            {
                selectedSymbolIndex = index;
            }
        }

        public void SetGridService(IGridService gridService)
        {
            this.gridService = gridService;
        }

        public string GetSelectedSymbol()
        {
            return selectedSymbolIndex >= 0 && selectedSymbolIndex < currentSymbols.Count 
                ? currentSymbols[selectedSymbolIndex] 
                : null;
        }

        public void ClearSelectedSymbol()
        {
            selectedSymbolIndex = -1;
            selectedSymbol = null;
        }

        public void ClearSymbolBank()
        {
            currentSymbols.Clear();
            ClearSelectedSymbol();
        }

        public void InjectSymbol(string symbol, int x, int y)
        {
            if (currentSymbols.Contains(symbol))
            {
                currentSymbols.Remove(symbol);
            }
        }

        public void SetSelectedSymbol(int index)
        {
            throw new System.NotImplementedException();
        }
    }
}