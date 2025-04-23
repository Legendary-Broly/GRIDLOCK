using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
public class EntropyService : IEntropyService
{
    public int EntropyPercent { get; private set; } = 0;

    public void Increase(int amount)
    {
        EntropyPercent = Mathf.Min(100, EntropyPercent + amount);
    }

    public void Decrease(int amount)
    {
        Debug.Log($"[EntropyService] Decrease called. Current Entropy: {EntropyPercent}, Amount: {amount}"); // Debug log
        EntropyPercent = Mathf.Max(0, EntropyPercent - amount);
        Debug.Log($"[EntropyService] New Entropy: {EntropyPercent}"); // Debug log
    }
}