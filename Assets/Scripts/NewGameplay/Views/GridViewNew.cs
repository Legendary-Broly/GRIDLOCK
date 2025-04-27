using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NewGameplay.Interfaces;

public class GridView : MonoBehaviour
{
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Transform gridParent;

    private Button[,] tileButtons;
    private IGridService gridService;

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
            gridService = bootstrapper.ExposedGridService;
            gridService.OnGridUpdated += () => RefreshGrid(gridService);
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

    private void OnDestroy()
    {
        if (gridService != null)
        {
            gridService.OnGridUpdated -= () => RefreshGrid(gridService);
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
                    var button = tileButtons[x, y];
                    if (button == null) continue;

                    var textComponents = button.GetComponentsInChildren<TextMeshProUGUI>();
                    
                    if (textComponents.Length < 2)
                    {
                        Debug.LogError($"Tile at ({x}, {y}) is missing required TextMeshProUGUI components");
                        continue;
                    }

                    var slashText = textComponents[0];
                    var symbolText = textComponents[1];

                    var symbol = service.GetSymbolAt(x, y);

                    if (symbol == "DF")
                    {
                        slashText.enabled = false;
                        symbolText.text = "⬢";  // Using a hexagon symbol for DF
                        symbolText.color = Color.yellow;
                        symbolText.fontSize = 40; // Ensure the symbol is clearly visible
                        symbolText.enabled = true; // Explicitly enable the text component
                        Debug.Log($"[GridView] Displaying DF symbol at ({x}, {y})");
                    }
                    else if (string.IsNullOrEmpty(symbol))
                    {
                        slashText.enabled = true;
                        symbolText.text = "";
                        symbolText.color = Color.white;
                    }
                    else
                    {
                        slashText.enabled = true;
                        symbolText.text = symbol;
                        symbolText.color = Color.white;
                    }

                    button.interactable = service.IsTilePlayable(x, y);
                }
            }
        }
    }
}