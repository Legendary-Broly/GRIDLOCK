using UnityEngine;
using NewGameplay.SplitGrid.Interfaces;
using NewGameplay.SplitGrid.Services;
using NewGameplay.SplitGrid.Views;
using NewGameplay.SplitGrid.Data;
using NewGameplay.SplitGrid.Controllers;
using NewGameplay.Interfaces;
using UnityEngine.EventSystems;

namespace NewGameplay.SplitGrid.Controllers
{
    public class SplitGridInputController : MonoBehaviour, IGridInputController
    {
        private ISplitGridService splitGridService;

        public void Initialize(ISplitGridService splitGridService)
        {
            this.splitGridService = splitGridService;
        }

        public void HandleTileClick(int x, int y, PointerEventData.InputButton button)
        {
            if (splitGridService == null) return;

            switch (button)
            {
                case PointerEventData.InputButton.Left:
                    if (splitGridService.CanRevealTile(GridID.GridA, x, y))
                        splitGridService.RevealTile(GridID.GridA, x, y);
                    break;
                case PointerEventData.InputButton.Right:
                    if (splitGridService.CanUseVirusFlag(GridID.GridA))
                        splitGridService.SetVirusFlag(GridID.GridA, x, y, !splitGridService.IsFlaggedAsVirus(GridID.GridA, x, y));
                    break;
            }
        }
    }
} 