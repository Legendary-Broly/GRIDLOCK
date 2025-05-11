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
        [SerializeField] private Image elementIcon;
        
        private IVirusService virusService;
        private ITileElementService tileElementService;
        private IGridService gridService;
        private Button tileButton;
        private int x, y;
        private GridInputController inputController;
        private ISymbolToolService symbolToolService;
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
            GridInputController inputController,
            ISymbolToolService symbolToolService = null)
        {
            this.virusService = virusService;
            this.tileElementService = elementService;
            this.gridService = gridService;
            this.x = x;
            this.y = y;
            this.inputController = inputController;
            this.symbolToolService = symbolToolService;

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
            if (gridService.GetTileState(x, y) != TileState.Revealed)
            {
                // Still hidden — do not show hint at all
                virusHintText.text = string.Empty;
                return;
            }

            int count = 0;
            Vector2Int[] dirs;
            if (symbolToolService != null && symbolToolService.IsPivotActive())
            
            {
                Debug.Log($"[TileSlotView] ({x},{y}) Refreshing virus hint — PivotActive: {symbolToolService?.IsPivotActive()}");
                dirs = new[] { new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1) };
            }
            else
            {
                dirs = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            }
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

            if (flagIcon != null)
            {
                flagIcon.gameObject.SetActive(isFlagged);
                if (isFlagged)
                    flagIcon.transform.SetAsLastSibling();
            }

            // 🔹 New: Apply TileElementSO visuals
            if (revealed)
            {
                var elementSO = tileElementService.GetElementSOAt(x, y);
                if (elementSO != null && elementSO.elementType != TileElementType.Empty)
                {
                    if (elementIcon != null)
                    {
                        elementIcon.sprite = elementSO.icon;
                        elementIcon.color = elementSO.displayColor;
                        elementIcon.gameObject.SetActive(true);
                        elementIcon.transform.SetAsLastSibling();
                    }
                }
                else
                {
                    if (elementIcon != null) elementIcon.gameObject.SetActive(false);

                }
            }
            else
            {
                if (elementIcon != null) elementIcon.gameObject.SetActive(false);

            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (inputController != null)
            {
                inputController.HandleTileClick(x, y, eventData.button);
            }
        }

        public void SetSymbolToolService(ISymbolToolService toolService)
        {
            this.symbolToolService = toolService;
        }
    }
}
