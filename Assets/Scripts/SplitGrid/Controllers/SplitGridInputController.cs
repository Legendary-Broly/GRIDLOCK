using UnityEngine;
using UnityEngine.EventSystems;
using SplitGrid.Interfaces;
using NewGameplay.Views;

namespace SplitGrid.Controllers
{
    public class SplitGridInputController : MonoBehaviour
    {
        [SerializeField] private GridViewNew gridViewA;
        [SerializeField] private GridViewNew gridViewB;

        private SplitGridController splitGridController;

        public void Initialize(SplitGridController controller)
        {
            splitGridController = controller;
        }

        public void HandleTileClick(int x, int y, PointerEventData.InputButton button)
        {
            // Determine which grid was clicked based on the view
            GridID gridId = DetermineClickedGrid(x, y);
            if (gridId == GridID.GridA || gridId == GridID.GridB)
            {
                bool isRightClick = button == PointerEventData.InputButton.Right;
                splitGridController.HandleTileClick(gridId, x, y, isRightClick);
            }
        }

        private GridID DetermineClickedGrid(int x, int y)
        {
            // Convert screen coordinates to local coordinates for each grid
            Vector2 localPointA = gridViewA.transform.InverseTransformPoint(Input.mousePosition);
            Vector2 localPointB = gridViewB.transform.InverseTransformPoint(Input.mousePosition);

            // Check if the click is within the bounds of each grid
            bool isInGridA = IsPointInGridBounds(localPointA, gridViewA);
            bool isInGridB = IsPointInGridBounds(localPointB, gridViewB);

            if (isInGridA) return GridID.GridA;
            if (isInGridB) return GridID.GridB;

            return GridID.GridA; // Default to GridA if somehow neither grid was hit
        }

        private bool IsPointInGridBounds(Vector2 localPoint, GridViewNew gridView)
        {
            RectTransform rectTransform = gridView.GetComponent<RectTransform>();
            if (rectTransform == null) return false;

            // Check if the point is within the RectTransform bounds
            return RectTransformUtility.RectangleContainsScreenPoint(
                rectTransform,
                Input.mousePosition,
                null
            );
        }
    }
} 