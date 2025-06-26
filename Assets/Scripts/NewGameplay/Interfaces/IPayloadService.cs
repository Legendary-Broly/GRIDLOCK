using System;
using NewGameplay.Enums;

namespace NewGameplay.Interfaces
{
    public interface IPayloadService
    {
        event Action OnPayloadActivated;
        event Action OnPayloadDeactivated;
        event Action OnExtractComplete;

        bool IsPayloadActive(PayloadType payloadType);
        void ActivatePayload(PayloadType payloadType);
        void DeactivatePayload(PayloadType payloadType);
        void ResetPayloads();
        void ExtractGrid();
    }
} 