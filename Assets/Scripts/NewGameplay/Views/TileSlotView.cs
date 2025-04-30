using UnityEngine;
using UnityEngine.UI;
using NewGameplay.Enums;
using NewGameplay.Interfaces;


public class TileSlotView : MonoBehaviour
{
    [SerializeField] private Image arrowTop;
    [SerializeField] private Image arrowBottom;
    [SerializeField] private Image arrowLeft;
    [SerializeField] private Image arrowRight;

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


    public void SetDirectionalIndicators(int x, int y)
    {
        int w = tileElementService.GridWidth;
        int h = tileElementService.GridHeight;

        SetArrow(arrowTop,    GetHintTypeAt(x, y - 1, w, h));
        SetArrow(arrowBottom, GetHintTypeAt(x, y + 1, w, h));
        SetArrow(arrowLeft,   GetHintTypeAt(x - 1, y, w, h));
        SetArrow(arrowRight,  GetHintTypeAt(x + 1, y, w, h));
    }

    private VisualTileHintType GetHintType(TileElementType type, int x, int y)
    {
        if (virusSpreadService != null && virusSpreadService.HasVirusAt(x, y))
            return VisualTileHintType.Virus;

        switch (type)
        {
            case TileElementType.ProgressTile:
            case TileElementType.EntropyReducer:
            case TileElementType.CodeShardConstructor:
            case TileElementType.CodeShardArgument:
            case TileElementType.CodeShardCloser:
                return VisualTileHintType.Good;

            case TileElementType.EntropyIncreaser:
            case TileElementType.VirusNest:
                return VisualTileHintType.Warning;

            default:
                return VisualTileHintType.None;
        }
    }

    private void SetArrow(Image arrow, VisualTileHintType type)
    {
        if (arrow == null) return;

        switch (type)
        {
            case VisualTileHintType.Virus:
                arrow.enabled = true;
                arrow.color = VirusRed;
                break;

            default:
                arrow.enabled = false;
                break;
        }
    }

    public void HideArrowTop()    { if (arrowTop    != null) arrowTop.enabled = false; }
    public void HideArrowBottom() { if (arrowBottom != null) arrowBottom.enabled = false; }
    public void HideArrowLeft()   { if (arrowLeft   != null) arrowLeft.enabled = false; }
    public void HideArrowRight()  { if (arrowRight  != null) arrowRight.enabled = false; }
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
