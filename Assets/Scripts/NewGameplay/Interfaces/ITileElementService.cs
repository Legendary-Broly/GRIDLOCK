using UnityEngine;
using NewGameplay.Enums;
using NewGameplay.ScriptableObjects;
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
        void ResizeGrid(int width, int height);
        public void AddManualElement(TileElementType elementType);
        void AddToSpawnPool(TileElementType element);
        void OnTileRevealed(int x, int y);
    }
}
