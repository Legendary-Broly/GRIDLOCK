using UnityEngine;
using System;
using NewGameplay.Enums;
using NewGameplay.ScriptableObjects;
namespace NewGameplay.Interfaces
{
    public interface ITileElementService
    {
        event Action<int, int> OnElementTriggered;
        
        int GridWidth { get; }
        int GridHeight { get; }
        void GenerateElements();
        TileElementType GetElementAt(int x, int y);
        void SetElementAt(int x, int y, TileElementType elementType);
        void TriggerElementEffect(int x, int y);
        TileElementSO GetElementSOAt(int x, int y);
        void ResizeGrid(int width, int height);
        void ClearElements();
        
        // Service Dependencies
        void SetGridService(IGridService service);
        void SetCodeShardTracker(ICodeShardTrackerService service);
        void SetInjectService(IInjectService service);
        void SetSystemIntegrityService(ISystemIntegrityService service);
        void SetVirusService(IVirusService service);
        void SetChatLogService(IChatLogService service);
        void SetDataFragmentService(IDataFragmentService service);
        void SetProgressTrackerService(IProgressTrackerService service);
        
        public void AddManualElement(TileElementType elementType);
        void AddToSpawnPool(TileElementType element);
        void OnTileRevealed(int x, int y);
    }
}
