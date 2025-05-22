using UnityEngine;

namespace NewGameplay.Views
{
    public class SplitGridView : MonoBehaviour
    {
        [Header("Grid Containers")]
        [SerializeField] private GameObject singleGridContainer;
        [SerializeField] private GameObject splitGridContainer;

        [Header("Individual Grids (Optional)")]
        [SerializeField] private GameObject gridAContainer;
        [SerializeField] private GameObject gridBContainer;

        /// <summary>
        /// Shows the split grid UI and hides the single grid view.
        /// </summary>
        public void ShowSplitGrid()
        {
            if (singleGridContainer != null)
                singleGridContainer.SetActive(false);

            if (splitGridContainer != null)
                splitGridContainer.SetActive(true);
        }

        /// <summary>
        /// Shows the single grid UI and hides the split grid view.
        /// </summary>
        public void ShowSingleGrid()
        {
            if (singleGridContainer != null)
                singleGridContainer.SetActive(true);

            if (splitGridContainer != null)
                splitGridContainer.SetActive(false);
        }

        /// <summary>
        /// Optionally toggle visibility of individual sub-grids (useful for debug or transitions).
        /// </summary>
        public void SetGridVisibility(bool showGridA, bool showGridB)
        {
            if (gridAContainer != null)
                gridAContainer.SetActive(showGridA);

            if (gridBContainer != null)
                gridBContainer.SetActive(showGridB);
        }
    }
}
