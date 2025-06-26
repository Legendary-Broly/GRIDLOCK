using UnityEngine;
using UnityEngine.EventSystems;

namespace NewGameplay.Interfaces
{
    public interface IGridInputController
    {
        void HandleTileClick(int x, int y, PointerEventData.InputButton button);
    }
} 