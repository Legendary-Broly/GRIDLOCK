using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GridView : MonoBehaviour
{
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Transform gridParent;

    private Button[,] tileButtons;

    public void BuildGrid(int size, System.Action<int, int> onClick)
    {
        // Get the actual grid dimensions from the service
        var bootstrapper = UnityEngine.Object.FindFirstObjectByType<NewGameplay.NewGameplayBootstrapper>();
        int width = size; // Default to size for compatibility
        int height = size; // Default to size for compatibility
        
        if (bootstrapper != null && bootstrapper.ExposedGridService != null)
        {
            width = bootstrapper.ExposedGridService.GridWidth;
            height = bootstrapper.ExposedGridService.GridHeight;
            Debug.Log($"[GridView] Building grid with dimensions: {width}x{height}");
        }
        else
        {
            Debug.LogWarning("[GridView] Couldn't get grid dimensions from service, using square grid as fallback");
        }
        
        // Initialize the button array with the correct dimensions
        tileButtons = new Button[width, height];

        // Create the grid of buttons
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject tile = Instantiate(tilePrefab, gridParent);
                int lx = x, ly = y;
                var button = tile.GetComponentInChildren<Button>();
                button.onClick.AddListener(() => onClick(lx, ly));
                tileButtons[x, y] = button;
            }
        }
    }
    
    public void RefreshGrid(IGridService service)
    {
        int width = service.GridWidth;
        int height = service.GridHeight;
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x < tileButtons.GetLength(0) && y < tileButtons.GetLength(1))
                {
                    var slashText = tileButtons[x, y].GetComponentsInChildren<TextMeshProUGUI>()[0];
                    var symbolText = tileButtons[x, y].GetComponentsInChildren<TextMeshProUGUI>()[1];

                    var symbol = service.GetSymbolAt(x, y);

                    if (string.IsNullOrEmpty(symbol))
                    {
                        slashText.enabled = true;
                        symbolText.text = "";
                    }
                    else
                    {
                        slashText.enabled = false;
                        symbolText.text = symbol;
                    }

                    tileButtons[x, y].interactable = service.IsTilePlayable(x, y);
                }
            }
        }
    }
}