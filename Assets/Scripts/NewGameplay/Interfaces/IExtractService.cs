using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public interface IExtractService
{
    void ExtractGrid();
    int CurrentScore { get; }
    event System.Action onGridUpdated;
    void ClearProtectedTiles();
}