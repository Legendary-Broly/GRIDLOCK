using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NewGameplay.Controllers;


public class InjectService : NewGameplay.Interfaces.IInjectService
{
    private readonly List<string> availableSymbols = new() { "∆", "Ψ", "Σ" };
    private readonly string[] currentSymbols = new string[3];
    private readonly System.Random rng = new();
    public string CurrentSymbolAt(int index) => currentSymbols[index];

    private int selectedIndex = -1;
    public string SelectedSymbol => selectedIndex >= 0 ? currentSymbols[selectedIndex] : null;

    public void InjectSymbols()
    {
        for (int i = 0; i < 3; i++)
            currentSymbols[i] = availableSymbols[rng.Next(availableSymbols.Count)];

        selectedIndex = -1;
    }

    public void SelectSymbol(int index)
    {
        if (index < 0 || index >= currentSymbols.Length) return;
        selectedIndex = index;
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
        Debug.Log("[InjectService] Clearing symbol bank");
        
        // Clear the internal state
        for (int i = 0; i < currentSymbols.Length; i++)
        {
            currentSymbols[i] = "";
        }
        selectedIndex = -1;
        // Notify the UI to update
        var controller = UnityEngine.Object.FindFirstObjectByType<InjectController>();
        if (controller != null)
        {
            controller.ClearSymbolSlots();
            controller.RefreshUI(); // Force a UI refresh after clearing
            Debug.Log("[InjectService] UI cleared and refreshed");
        }
        else
        {
            Debug.LogError("[InjectService] Could not find InjectController!");
        }
    }

    public void ClearSelectedSymbol()
    {
        selectedIndex = -1;
    }

    public string GetSelectedSymbol()
    {
        throw new NotImplementedException();
    }
}