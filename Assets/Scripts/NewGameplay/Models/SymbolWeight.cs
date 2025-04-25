using UnityEngine;

namespace NewGameplay.Models
{
    public class SymbolWeight
    {
        public string Symbol { get; }
        public float BaseWeight { get; }
        public float MinWeight { get; }
        public float MaxWeight { get; }

        public SymbolWeight(string symbol, float baseWeight, float minWeight, float maxWeight)
        {
            Symbol = symbol;
            BaseWeight = baseWeight;
            MinWeight = minWeight;
            MaxWeight = maxWeight;
        }

        public float GetWeightAtEntropy(float entropyPercent)
        {
            float t = Mathf.Clamp01(entropyPercent / 100f);
            return Mathf.Lerp(BaseWeight, MaxWeight, t);
        }
    }
} 