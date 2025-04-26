using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public interface IGridService
{
    event Action OnGridUpdated;
    void TryPlaceSymbol(int x, int y);
    void SpreadVirus();
    void SetSymbol(int x, int y, string symbol);
    string GetSymbolAt(int x, int y);
    bool IsTilePlayable(int x, int y);
    int GridSize { get; }
    int GridWidth { get; }
    int GridHeight { get; }
    void ClearAllExceptViruses();
    void ClearAllTiles();
    string[,] GridState { get; }
    bool[,] TilePlayable { get; }
    void ProcessPurges();
    void EnableRowColumnPurge();
    void DisableRowColumnPurge();
    void TriggerGridUpdate();
}