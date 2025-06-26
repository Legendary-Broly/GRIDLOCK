using NewGameplay.Enums;
using NewGameplay.Interfaces;

namespace NewGameplay.Interfaces
{
    public interface ISplitGridService
    {
        IGridService GetGrid(GridID id);
        IVirusService GetVirusService(GridID id);
        ITileElementService GetTileElementService(GridID id);
        IDataFragmentService GetFragmentService(GridID id);
        void ApplyPayload(PayloadType payloadType);
        void AddTileElement(TileElementType elementType);

    }
}
