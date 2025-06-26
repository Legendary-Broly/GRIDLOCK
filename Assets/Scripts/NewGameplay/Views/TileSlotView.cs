using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using NewGameplay.Enums;
using NewGameplay.Interfaces;
using NewGameplay.Models;

namespace NewGameplay.Views
{
    public class TileSlotView : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI virusHintText;
        [SerializeField] private GameObject playerRevealHighlight;
        [SerializeField] private GameObject adjacencyHighlight;
        [SerializeField] private TextMeshProUGUI symbolText;
        [SerializeField] private Image flagIcon;
        [SerializeField] private Image elementIcon;
        [SerializeField] private Image tileBackground;

        private Button tileButton;
        private int x, y;
        private Color originalColor;
        private Coroutine pulseCoroutine;

        private IVirusService virusService;
        private ITileElementService tileElementService;
        private IGridService gridService;
        private IDataFragmentService dataFragmentService;
        private ISymbolToolService symbolToolService;

        private System.Action<int, int, PointerEventData.InputButton> onTileClick;

        public event Action<int, int, PointerEventData.InputButton> OnTileClicked;

        public void Initialize(
            int x,
            int y,
            IGridService gridService,
            IVirusService virusService,
            ITileElementService elementService,
            ISymbolToolService toolService,
            System.Action<int, int, PointerEventData.InputButton> onTileClick)
        {
            this.x = x;
            this.y = y;
            this.gridService = gridService ?? throw new ArgumentNullException(nameof(gridService));
            this.virusService = virusService ?? throw new ArgumentNullException(nameof(virusService));
            this.tileElementService = elementService ?? throw new ArgumentNullException(nameof(elementService));
            this.symbolToolService = toolService ?? throw new ArgumentNullException(nameof(toolService));
            this.onTileClick = onTileClick;

            InitializeComponents();
            ResetVisualState();

            tileButton.onClick.AddListener(() => onTileClick?.Invoke(x, y, PointerEventData.InputButton.Left));
        }

        private void InitializeComponents()
        {
            tileButton = GetComponentInChildren<Button>();
            if (tileBackground == null)
                tileBackground = GetComponent<Image>();
            if (tileBackground != null)
                originalColor = tileBackground.color;
        }

        private void ResetVisualState()
        {
            symbolText?.gameObject.SetActive(false);
            playerRevealHighlight?.SetActive(false);
            adjacencyHighlight?.SetActive(false);
        }

        public void SetDataFragmentService(IDataFragmentService service) => dataFragmentService = service ?? throw new ArgumentNullException(nameof(service));

        public void OnPointerClick(PointerEventData eventData)
        {
            OnTileClicked?.Invoke(x, y, eventData.button);
        }

        public void Refresh()
        {
            UpdateVirusHint();
            UpdateRevealState();
            UpdateInteractionState();
            UpdateElementVisual();
        }

        private void UpdateVirusHint()
        {
            if (gridService.GetTileState(x, y) != TileState.Revealed)
            {
                virusHintText.text = string.Empty;
                return;
            }

            int count = CountAdjacentViruses();
            virusHintText.text = count > 0 ? new string('.', count) : string.Empty;
        }

        private int CountAdjacentViruses()
        {
            int count = 0;
            Vector2Int[] dirs = symbolToolService?.IsPivotActive() == true
                ? new[] { new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1) }
                : new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (var dir in dirs)
            {
                int nx = x + dir.x;
                int ny = y + dir.y;
                if (gridService.IsInBounds(nx, ny) && virusService.HasVirusAt(nx, ny))
                    count++;
            }

            return count;
        }

        private void UpdateRevealState()
        {
            bool revealed = gridService.GetTileState(x, y) == TileState.Revealed;
            playerRevealHighlight?.SetActive(revealed);

            if (revealed)
            {
                CheckAndPulseForFragmentProximity();
                UpdateSymbolText(gridService.GetSymbolAt(x, y));
            }
            else
            {
                symbolText?.gameObject.SetActive(false);
            }
        }

        private void UpdateInteractionState()
        {
            bool canReveal = gridService.CanRevealTile(x, y);
            bool isFlagged = gridService.IsFlaggedAsVirus(x, y);
            adjacencyHighlight?.SetActive(canReveal && !isFlagged);
            tileButton.interactable = canReveal;

            flagIcon?.gameObject.SetActive(isFlagged);
            if (isFlagged)
                flagIcon.transform.SetAsLastSibling();
        }

        private void UpdateSymbolText(string symbol)
        {
            if (symbolText == null) return;

            if (string.IsNullOrEmpty(symbol))
            {
                symbolText.gameObject.SetActive(false);
                return;
            }

            symbolText.gameObject.SetActive(true);
            symbolText.text = symbol;

            if (!string.IsNullOrEmpty(symbol))
                symbolText.transform.SetAsLastSibling();
        }

        private void UpdateElementVisual()
        {
            if (elementIcon == null) return;

            bool revealed = gridService.GetTileState(x, y) == TileState.Revealed;
            if (revealed)
            {
                var elementSO = tileElementService.GetElementSOAt(x, y);
                if (elementSO != null && elementSO.elementType != TileElementType.Empty)
                {
                    elementIcon.sprite = elementSO.icon;
                    elementIcon.color = elementSO.displayColor;
                    elementIcon.gameObject.SetActive(true);
                    elementIcon.transform.SetAsLastSibling();
                    return;
                }
            }

            elementIcon.gameObject.SetActive(false);
        }

        private void CheckAndPulseForFragmentProximity()
        {
            if (!gridService.IsTileRevealed(x, y) || gridService.GetSymbolAt(x, y) == "DATA") return;

            int distance = GetDistanceToNearestFragment();
            if (distance == 1)
                pulseCoroutine = StartCoroutine(PulseTile(new Color(0, 1, 0, 0.25f)));
            else if (distance == 2)
                pulseCoroutine = StartCoroutine(PulseTile(new Color(1, 1, 0, 0.25f)));
        }

        private int GetDistanceToNearestFragment()
        {
            if (dataFragmentService == null) return -1;

            int minDist = int.MaxValue;
            for (int gy = 0; gy < gridService.GridHeight; gy++)
            {
                for (int gx = 0; gx < gridService.GridWidth; gx++)
                {
                    if (dataFragmentService.IsFragmentAt(new Vector2Int(gx, gy)) && !gridService.IsTileRevealed(gx, gy))
                    {
                        int dist = Mathf.Abs(gx - x) + Mathf.Abs(gy - y);
                        minDist = Mathf.Min(minDist, dist);
                    }
                }
            }

            return minDist == int.MaxValue ? -1 : minDist;
        }

        private IEnumerator PulseTile(Color pulseColor, float duration = 0.5f)
        {
            if (tileBackground == null) yield break;

            if (pulseCoroutine != null)
                StopCoroutine(pulseCoroutine);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float alpha = Mathf.Sin(t * Mathf.PI);
                tileBackground.color = Color.Lerp(originalColor, pulseColor, alpha);
                elapsed += Time.deltaTime;
                yield return null;
            }

            tileBackground.color = originalColor;
            pulseCoroutine = null;
        }

        public void UpdateVisuals()
        {
            if (gridService == null) return;

            var state = gridService.GetTileState(x, y);
            var symbol = gridService.GetSymbolAt(x, y);
            var isFlagged = gridService.IsFlaggedAsVirus(x, y);

            UpdateBackground(state);
            UpdateSymbol(symbol);
            UpdateFlag(isFlagged);
        }

        private void UpdateBackground(TileState state)
        {
            if (tileBackground == null) return;

            switch (state)
            {
                case TileState.Revealed:
                    tileBackground.color = Color.white;
                    break;
                case TileState.Hidden:
                    tileBackground.color = Color.gray;
                    break;
                case TileState.Flagged:
                    tileBackground.color = Color.red;
                    break;
            }
        }

        private void UpdateSymbol(string symbol)
        {
            if (symbolText == null) return;

            if (string.IsNullOrEmpty(symbol))
            {
                symbolText.gameObject.SetActive(false);
                return;
            }

            symbolText.gameObject.SetActive(true);
            symbolText.text = symbol;
        }

        private void UpdateFlag(bool isFlagged)
        {
            if (flagIcon == null) return;
            flagIcon.gameObject.SetActive(isFlagged);
        }

        private void OnDestroy()
        {
            if (tileButton != null)
                tileButton.onClick.RemoveAllListeners();
        }
    }
}
