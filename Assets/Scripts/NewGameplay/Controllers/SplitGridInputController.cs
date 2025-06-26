using UnityEngine;
using UnityEngine.EventSystems;
using NewGameplay.Enums;
using NewGameplay.Controllers;
using NewGameplay.Services;

namespace NewGameplay.Controllers
{
    public class SplitGridInputController : MonoBehaviour
    {
        [SerializeField] private GridInputController gridInputA;
        [SerializeField] private GridInputController gridInputB;
        [SerializeField] private SplitGridController splitGridController;

        public void Initialize(GridInputController a, GridInputController b, SplitGridController splitController)
        {
            gridInputA = a;
            gridInputB = b;
            splitGridController = splitController;
        }

        public void HandleTileClick(int x, int y, GridID gridId, PointerEventData.InputButton button)
        {
            if (gridId == GridID.A)
            {
                gridInputA.HandleTileClick(x, y, button);
            }
            else if (gridId == GridID.B)
            {
                gridInputB.HandleTileClick(x, y, button);
            }

            if (button == PointerEventData.InputButton.Right)
            {
                splitGridController.AttemptFlag(gridId, x, y);
            }
        }

        public void HandleLeftClick(int x, int y, GridID gridId)
        {
            HandleTileClick(x, y, gridId, PointerEventData.InputButton.Left);
        }

        public void HandleRightClick(int x, int y, GridID gridId)
        {
            HandleTileClick(x, y, gridId, PointerEventData.InputButton.Right);
        }
    }
}
