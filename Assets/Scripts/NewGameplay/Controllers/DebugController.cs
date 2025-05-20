using UnityEngine;
using UnityEngine.UI;
using NewGameplay.Interfaces;
using NewGameplay.Services;
using System.Collections.Generic;

namespace NewGameplay.Controllers
{
    public class DebugController : MonoBehaviour
    {
        [SerializeField] private Button revealFragmentButton;
        private IGridService gridService;
        private IDataFragmentService dataFragmentService;
        private IProgressTrackerService progressService;

        private void Start()
        {
            if (revealFragmentButton != null)
            {
                revealFragmentButton.onClick.AddListener(OnRevealFragmentButtonClicked);
            }
        }

        public void Initialize(IGridService gridService, IDataFragmentService dataFragmentService, IProgressTrackerService progressService)
        {
            this.gridService = gridService;
            this.dataFragmentService = dataFragmentService;
            this.progressService = progressService;
        }

        private void OnRevealFragmentButtonClicked()
        {
            if (gridService == null || dataFragmentService == null) return;

            // Find all positions with data fragments that aren't revealed yet
            var hiddenFragments = new List<Vector2Int>();
            for (int y = 0; y < gridService.GridHeight; y++)
            {
                for (int x = 0; x < gridService.GridWidth; x++)
                {
                    var pos = new Vector2Int(x, y);
                    if (dataFragmentService.IsFragmentAt(pos) && !gridService.IsTileRevealed(x, y))
                    {
                        hiddenFragments.Add(pos);
                    }
                }
            }

            if (hiddenFragments.Count == 0)
            {
                Debug.Log("[DebugController] No hidden data fragments found on the board");
                return;
            }

            // Pick a random hidden fragment to reveal
            var randomPos = hiddenFragments[Random.Range(0, hiddenFragments.Count)];
            gridService.RevealTile(randomPos.x, randomPos.y, true);
            progressService.NotifyFragmentRevealed(randomPos.x, randomPos.y);
            
            Debug.Log($"[DebugController] Revealed data fragment at ({randomPos.x}, {randomPos.y})");
        }
    }
} 