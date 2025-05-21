using NewGameplay.Enums;
using NewGameplay.Interfaces;
using NewGameplay.Controllers;

namespace NewGameplay.Services
{
    public class SplitGridServiceLocator : ISplitGridService
    {
        private readonly IGridService gridServiceA;
        private readonly IGridService gridServiceB;
        private readonly IVirusService virusServiceA;
        private readonly IVirusService virusServiceB;
        private readonly ITileElementService tileElementServiceA;
        private readonly ITileElementService tileElementServiceB;
        private readonly IDataFragmentService fragmentServiceA;
        private readonly IDataFragmentService fragmentServiceB;
        private readonly PayloadManager payloadManager;
        public SplitGridServiceLocator(
            IGridService gridServiceA,
            IGridService gridServiceB,
            IVirusService virusServiceA,
            IVirusService virusServiceB,
            ITileElementService tileElementServiceA,
            ITileElementService tileElementServiceB,
            IDataFragmentService fragmentServiceA,
            IDataFragmentService fragmentServiceB,
            PayloadManager payloadManager
        )
        {
            this.gridServiceA = gridServiceA;
            this.gridServiceB = gridServiceB;
            this.virusServiceA = virusServiceA;
            this.virusServiceB = virusServiceB;
            this.tileElementServiceA = tileElementServiceA;
            this.tileElementServiceB = tileElementServiceB;
            this.fragmentServiceA = fragmentServiceA;
            this.fragmentServiceB = fragmentServiceB;
            this.payloadManager = payloadManager;
        }

        public IGridService GetGrid(GridID id)
        {
            return id == GridID.A ? gridServiceA : gridServiceB;
        }

        public IVirusService GetVirusService(GridID id)
        {
            return id == GridID.A ? virusServiceA : virusServiceB;
        }

        public ITileElementService GetTileElementService(GridID id)
        {
            return id == GridID.A ? tileElementServiceA : tileElementServiceB;
        }

        public IDataFragmentService GetFragmentService(GridID id)
        {
            return id == GridID.A ? fragmentServiceA : fragmentServiceB;
        }
        public void ApplyPayload(PayloadType payloadType)
        {
            payloadManager?.ActivatePayload(payloadType);
        }

        public void AddTileElement(TileElementType elementType)
        {
            tileElementServiceA.AddToSpawnPool(elementType);
            tileElementServiceB.AddToSpawnPool(elementType);

            tileElementServiceA.AddManualElement(elementType);
            tileElementServiceB.AddManualElement(elementType);
        }
    }
}
