using System;
using System.Collections.Generic;
using NewGameplay.Interfaces;
using NewGameplay.Enums;

namespace NewGameplay.Services
{
    public class PayloadService : IPayloadService
    {
        private readonly HashSet<PayloadType> activePayloads = new HashSet<PayloadType>();

        public event Action OnPayloadActivated;
        public event Action OnPayloadDeactivated;
        public event Action OnExtractComplete;

        public bool IsPayloadActive(PayloadType payloadType) => activePayloads.Contains(payloadType);

        public void ActivatePayload(PayloadType payloadType)
        {
            if (activePayloads.Add(payloadType))
            {
                OnPayloadActivated?.Invoke();
            }
        }

        public void DeactivatePayload(PayloadType payloadType)
        {
            if (activePayloads.Remove(payloadType))
            {
                OnPayloadDeactivated?.Invoke();
            }
        }

        public void ResetPayloads()
        {
            var payloads = new List<PayloadType>(activePayloads);
            foreach (var payload in payloads)
            {
                DeactivatePayload(payload);
            }
        }

        public void ExtractGrid()
        {
            // Extraction logic here
            OnExtractComplete?.Invoke();
        }
    }
} 