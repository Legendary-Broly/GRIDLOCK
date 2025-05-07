using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NewGameplay.Enums;
using NewGameplay.Interfaces;
using NewGameplay.Models;
using UnityEngine.EventSystems;
using NewGameplay.Controllers;

namespace NewGameplay.Views
{
    public class TileSlotView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI virusHintText;
        [SerializeField] private GameObject playerRevealHighlight;
        [SerializeField] private GameObject adjacencyHighlight;
        [SerializeField] private TextMeshProUGUI symbolText;
        [SerializeField] private Image flagIcon;
        private IVirusService virusService;
        private ITileElementService tileElementService;
        private IGridService gridService;
        private Button tileButton;
        private int x, y;
        private GridInputController inputController;
        public enum VisualTileHintType
        {
            None,
            Virus,
            Good,
            Warning
        }

        public void Initialize(
            IVirusService virusService,
            ITileElementService elementService,
            IGridService gridService,
            int x,
            int y,
            GridInputController inputController)
        {
            this.virusService = virusService;
            this.tileElementService = elementService;
            this.gridService = gridService;
            this.x = x;
            this.y = y;
            this.inputController = inputController;

            tileButton = GetComponentInChildren<Button>();

            if (symbolText == null)
            {
                Debug.LogError($"[TileSlotView] symbolText is not assigned on {gameObject.name}");
            }
            else
            {
                symbolText.transform.SetAsLastSibling();
                symbolText.gameObject.SetActive(false);
            }

            playerRevealHighlight?.SetActive(false);
            adjacencyHighlight?.SetActive(false);
        }

        private void SetVirusHintCount(int x, int y)
        {
            // Only show hints for revealed tiles
            if (gridService.GetTileState(x, y) != TileState.Revealed)
            {
                virusHintText.text = string.Empty;
                return;
            }

            int count = 0;
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (var dir in dirs)
            {
                int nx = x + dir.x;
                int ny = y + dir.y;
                if (gridService.IsInBounds(nx, ny) && virusService.HasVirusAt(nx, ny))
                    count++;
            }
            virusHintText.text = count > 0 ? new string('.', count) : string.Empty;
        }

        public void Refresh()
        {
            SetVirusHintCount(x, y);

            bool revealed = gridService.GetTileState(x, y) == TileState.Revealed;
            bool wasActive = playerRevealHighlight != null && playerRevealHighlight.activeSelf;
            playerRevealHighlight?.SetActive(revealed);

            // Only log when a tile transitions from not revealed to revealed
            if (revealed && !wasActive)
            {
                var sym = gridService.GetSymbolAt(x, y);
                Debug.Log($"[TileSlotView] ({x},{y}) transitioned to revealed: symbol='{sym}', gridService: {gridService.GetHashCode()} at {Time.time}");
            }

            bool canReveal = gridService.CanRevealTile(x, y);
            bool isFlagged = gridService.IsFlaggedAsVirus(x, y);
            adjacencyHighlight?.SetActive(canReveal && !isFlagged);


            if (tileButton != null)
                tileButton.interactable = canReveal;

            if (symbolText != null)
            {
                if (revealed)
                {
                    var sym = gridService.GetSymbolAt(x, y);
                    bool show = !string.IsNullOrEmpty(sym);
                    symbolText.text = sym;
                    symbolText.gameObject.SetActive(show);
                    if (show)
                        symbolText.transform.SetAsLastSibling();
                }
                else
                {
                    symbolText.gameObject.SetActive(false);
                }
            }

            // 🔻 ADD THIS BLOCK TO HANDLE FLAG DISPLAY 🔻
            if (flagIcon != null)
            {
                flagIcon.gameObject.SetActive(isFlagged);
                if (isFlagged)
                    flagIcon.transform.SetAsLastSibling();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (inputController != null)
            {
                inputController.HandleTileClick(x, y, eventData.button);
            }
        }
    }
}
