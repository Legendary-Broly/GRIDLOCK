using UnityEngine;
using UnityEngine.UI;
using NewGameplay.Enums;
using NewGameplay.Interfaces;
using TMPro;

public class TileSlotView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI virusHintText;
    [SerializeField] private GameObject playerRevealHighlight;

    private IVirusSpreadService virusSpreadService;
    private ITileElementService tileElementService;
    private IGridService gridService;
    private static readonly Color VirusRed = new Color(1f, 0.2f, 0.2f);

    public enum VisualTileHintType
    {
        None,
        Virus,
        Good,
        Warning
    }

    public void Initialize(IVirusSpreadService virusService, ITileElementService elementService, IGridService gridService)
    {
        this.virusSpreadService = virusService;
        this.tileElementService = elementService;
        this.gridService = gridService;
    }

    public void SetVirusHintCount(int x, int y)
    {
        int count = 0;

        Vector2Int[] dirs = new[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (var dir in dirs)
        {
            int nx = x + dir.x;
            int ny = y + dir.y;

            if (gridService.IsInBounds(nx, ny) && virusSpreadService.HasVirusAt(nx, ny))
            {
                count++;
            }
        }

        virusHintText.text = count > 0 ? count.ToString() : "";
    }

    public void SetPlayerRevealed(bool state)
    {
        if (playerRevealHighlight != null)
            playerRevealHighlight.SetActive(state);
    }

    private VisualTileHintType GetHintType(TileElementType type, int x, int y)
    {
        if (virusSpreadService != null && virusSpreadService.HasVirusAt(x, y))
            return VisualTileHintType.Virus;

        switch (type)
        {
            case TileElementType.ProgressTile:
            case TileElementType.EntropyReducer:
            case TileElementType.CodeShard:
                return VisualTileHintType.Good;

            case TileElementType.EntropyIncreaser:
            case TileElementType.VirusNest:
                return VisualTileHintType.Warning;

            default:
                return VisualTileHintType.None;
        }
    }
    private VisualTileHintType GetHintTypeAt(int x, int y, int w, int h)
    {
        if (x < 0 || x >= w || y < 0 || y >= h)
            return VisualTileHintType.None;

        var element = tileElementService.GetElementAt(x, y);

        if (virusSpreadService != null && virusSpreadService.HasVirusAt(x, y))
            return VisualTileHintType.Virus;

        return GetHintType(element, x, y);
    }

}
