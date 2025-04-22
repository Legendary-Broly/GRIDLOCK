using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public interface IEntropyService
{
    int EntropyPercent { get; }
    void Increase(int amount);
    void Decrease(int amount);
}