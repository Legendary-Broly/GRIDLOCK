using UnityEngine;
using NewGameplay.Enums;
using NewGameplay.ScriptableObjects;

namespace SplitGrid.Interfaces
{
    public interface ISplitTileElementService
    {
        // Grid Initialization
        void InitializeGrids(int widthA, int heightA, int widthB, int heightB);
        void GenerateElements();
        
        // Element Operations
        void TriggerElementEffect(GridID gridId, int x, int y);
        TileElementType GetElementAt(GridID gridId, int x, int y);
        TileElementSO GetElementSOAt(GridID gridId, int x, int y);
        
        // Element Management
        void AddManualElement(GridID gridId, TileElementType elementType);
        void AddToSpawnPool(GridID gridId, TileElementType element);
        void OnTileRevealed(GridID gridId, int x, int y);
        
        // Grid State
        void ResizeGrid(GridID gridId, int width, int height);
        void ClearElements(GridID gridId);

        // Payload Effects
        void ApplyPayloadEffect(PayloadType payloadType);
    }
} 