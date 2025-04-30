using UnityEngine;
using NewGameplay.Enums;
namespace NewGameplay.Interfaces
{
    public interface ITileElementService
    {
        int GridWidth { get; }
        int GridHeight { get; }

        void GenerateElements();
        TileElementType GetElementAt(int x, int y);
        void TriggerElementEffect(int x, int y);
        TileElementSO GetElementSOAt(int x, int y);
        Vector2Int? GetVirusNestPosition();
        void TriggerElementEffectForFirstVirus();
    }
}

