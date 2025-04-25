using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NewGameplay.Models;
using NewGameplay.Interfaces;

namespace NewGameplay.Services
{
    public class WeightedInjectService : IWeightedInjectService
    {
        private readonly Dictionary<string, SymbolWeight> symbolWeights;
        private readonly string[] currentSymbols;
        private readonly System.Random rng;
        private readonly Dictionary<string, int> consecutiveMisses;
        private int selectedIndex = -1;
        private IGridService gridService;

        public event Action<string[]> OnSymbolsInjected;

        public string SelectedSymbol => selectedIndex >= 0 ? currentSymbols[selectedIndex] : null;

        public WeightedInjectService()
        {
            rng = new System.Random();
            currentSymbols = new string[3];
            consecutiveMisses = new Dictionary<string, int>();
            
            // Initialize symbol weights with base, min, and max weights
            symbolWeights = new Dictionary<string, SymbolWeight>
            {
                { "Ψ", new SymbolWeight("Ψ", 10f, 10f, 10f) },
                { "∆", new SymbolWeight("∆", 10f, 10f, 60f) },
                { "Θ", new SymbolWeight("Θ", 45f, 45f, 55f) },
                { "Σ", new SymbolWeight("Σ", 10f, 10f, 10f) }
            };

            // Initialize consecutive misses tracking
            foreach (var symbol in symbolWeights.Keys)
            {
                consecutiveMisses[symbol] = 0;
            }
        }

        public void SetGridService(IGridService service)
        {
            gridService = service;
        }

        private int CountVirusesOnGrid()
        {
            if (gridService == null) return 0;

            int count = 0;
            for (int y = 0; y < gridService.GridSize; y++)
            {
                for (int x = 0; x < gridService.GridSize; x++)
                {
                    if (gridService.GetSymbolAt(x, y) == "X")
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public void InjectSymbols()
        {
            for (int i = 0; i < 3; i++)
            {
                currentSymbols[i] = DrawWeightedSymbol();
            }

            // Update consecutive misses
            var drawnSymbols = new HashSet<string>(currentSymbols);
            foreach (var symbol in symbolWeights.Keys)
            {
                if (!drawnSymbols.Contains(symbol))
                {
                    consecutiveMisses[symbol]++;
                }
                else
                {
                    consecutiveMisses[symbol] = 0;
                }
            }

            selectedIndex = -1;
            OnSymbolsInjected?.Invoke(currentSymbols);
        }

        private string DrawWeightedSymbol()
        {
            var adjustedWeights = new Dictionary<string, float>();
            float totalWeight = 0f;

            // Count viruses to adjust purge weight
            int virusCount = CountVirusesOnGrid();

            foreach (var kvp in symbolWeights)
            {
                float weight = kvp.Value.BaseWeight;
                
                // Adjust purge symbol weight based on virus count
                if (kvp.Key == "∆")
                {
                    // Scale weight more aggressively: base weight + (virus count * 5)
                    // This means each virus adds 5 to the weight
                    weight = Mathf.Min(60f, weight + (virusCount * 5f));
                    Debug.Log($"[∆] Adjusted weight to {weight} based on {virusCount} viruses");
                }
                // Apply soft guarantees
                else if (kvp.Key == "∆" && consecutiveMisses[kvp.Key] >= 3)
                {
                    weight *= 1.5f; // +50% boost
                }
                else if (kvp.Key == "Σ" && consecutiveMisses[kvp.Key] >= 4)
                {
                    weight *= 1.7f; // +70% boost
                }

                adjustedWeights[kvp.Key] = weight;
                totalWeight += weight;
            }

            float roll = (float)(rng.NextDouble() * totalWeight);
            float currentSum = 0f;

            foreach (var kvp in adjustedWeights)
            {
                currentSum += kvp.Value;
                if (roll <= currentSum)
                {
                    return kvp.Key;
                }
            }

            return adjustedWeights.Keys.First(); // Fallback
        }

        public void SelectSymbol(int index)
        {
            if (index >= 0 && index < currentSymbols.Length)
            {
                selectedIndex = index;
            }
        }

        public void ClearSelectedSymbol(string symbol)
        {
            for (int i = 0; i < currentSymbols.Length; i++)
            {
                if (currentSymbols[i] == symbol)
                {
                    currentSymbols[i] = "";
                    if (selectedIndex == i) selectedIndex = -1;
                    break;
                }
            }
        }

        public void ClearSymbolBank()
        {
            for (int i = 0; i < currentSymbols.Length; i++)
            {
                currentSymbols[i] = "";
            }
            selectedIndex = -1;
        }

        public void ClearSelectedSymbol()
        {
            selectedIndex = -1;
        }

        public float GetSymbolWeight(string symbol)
        {
            return symbolWeights.TryGetValue(symbol, out var weight) ? weight.BaseWeight : 0f;
        }

        public void UpdateWeights(float entropyPercent)
        {
            foreach (var weight in symbolWeights.Values)
            {
                weight.GetWeightAtEntropy(entropyPercent);
            }
        }

        public string[] GetCurrentSymbols()
        {
            return currentSymbols.ToArray();
        }
    }
} 