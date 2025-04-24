using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class InjectService : IInjectService
{
    private readonly List<string> availableSymbols = new() { "∆", "Θ", "Ψ", "Σ" };
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
        var controller = GameObject.FindFirstObjectByType<InjectController>();
        if (controller != null)
        {
            controller.ClearSymbolSlots(); // Call the controller to clear the UI slots
        }
    }

    public void ClearSelectedSymbol()
    {
        selectedIndex = -1;
    }
}