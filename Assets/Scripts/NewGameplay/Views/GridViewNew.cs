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
        tileButtons = new Button[size, size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
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
        for (int y = 0; y < service.GridSize; y++)
        {
            for (int x = 0; x < service.GridSize; x++)
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