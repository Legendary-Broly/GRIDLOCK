// TileModifierAssigner.cs
using UnityEngine;
using System.Collections.Generic;

public class TileModifierAssigner : MonoBehaviour
{
    [SerializeField] private List<TileModifierSO> availableModifiers;
    [SerializeField] private int minModifiers = 1;
    [SerializeField] private int maxModifiers = 3;

    public void AssignModifiers(List<TileSlotController> tiles)
    {
        if (availableModifiers == null || availableModifiers.Count == 0 || tiles == null || tiles.Count == 0)
        {
            // Debug.LogWarning("TileModifierAssigner: No modifiers or tiles available.");
            return;
        }

        int modifierCount = Random.Range(minModifiers, maxModifiers + 1);
        // Debug.Log($"Attempting to assign {modifierCount} modifiers.");

        List<int> availableIndexes = new List<int>();
        for (int i = 0; i < tiles.Count; i++) availableIndexes.Add(i);

        for (int i = 0; i < modifierCount; i++)
        {
            if (availableIndexes.Count == 0) break;

            int tileIndex = availableIndexes[Random.Range(0, availableIndexes.Count)];
            availableIndexes.Remove(tileIndex);

            TileSlotController tile = tiles[tileIndex];
            TileModifierSO modifier = availableModifiers[Random.Range(0, availableModifiers.Count)];
            
            // Debug.Log($"Assigning modifier {modifier.modifierType} with value {modifier.amount} to tile {tileIndex}");
            
            tile.AssignModifier(modifier);
        }
    }
}
