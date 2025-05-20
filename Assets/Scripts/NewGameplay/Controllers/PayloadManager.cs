using System.Collections.Generic;
using UnityEngine;
using NewGameplay.Models;
using NewGameplay.Enums;
using System;

namespace NewGameplay.Controllers
{
    public class PayloadManager : MonoBehaviour
    {
        private Dictionary<PayloadType, Payload> availablePayloads;
        private List<Payload> activePayloads;

        private void Awake()
        {
            InitializePayloads();
        }

        private void InitializePayloads()
        {
            availablePayloads = new Dictionary<PayloadType, Payload>
            {
                { PayloadType.DataCluster, new Payload(PayloadType.DataCluster, "Data Cluster", "Data fragments are more likely to spawn in clusters") },
                { PayloadType.Phishing, new Payload(PayloadType.Phishing, "Phishing", "Revealing a virus reveals <color=#E44E4E>2 additional, random tiles</color> on the grid as well") },
                { PayloadType.Echo, new Payload(PayloadType.Echo, "Echo", "Revealed tiles have a <color=#E44E4E>30% chance</color> to stay revealed next round") },
                { PayloadType.ToolkitExpansion, new Payload(PayloadType.ToolkitExpansion, "Toolkit Expansion", "Fourth random tool added to toolkit") },
                { PayloadType.DamageOverTime, new Payload(PayloadType.DamageOverTime, "Damage Over Time", "System integrity loss is spread across <color=#E44E4E>3 reveals</color> rather than immediately") },
                { PayloadType.WirelessUpload, new Payload(PayloadType.WirelessUpload, "Wireless Uplink", "Revealing a tile element has a <color=#E44E4E>50% chance</color> to reveal the closest tile with the same element") }
            };

            activePayloads = new List<Payload>();
        }

        public void ActivatePayload(PayloadType type)
        {
            if (availablePayloads.TryGetValue(type, out Payload payload) && !payload.IsActive)
            {
                payload.Activate();
                activePayloads.Add(payload);
            }
            switch (type)
            {
                case PayloadType.ToolkitExpansion:
                    var controller = UnityEngine.Object.FindFirstObjectByType<InjectController>();
                    if (controller != null)
                        controller.EnableNextToolSlot();
                        controller.AssignRandomToolToNextSlot();
                    break;
                
            }


        }

        public void DeactivatePayload(PayloadType type)
        {
            if (availablePayloads.TryGetValue(type, out Payload payload) && payload.IsActive)
            {
                payload.Deactivate();
                activePayloads.Remove(payload);

            }

        }

        public bool IsPayloadActive(PayloadType type)
        {
            bool active = availablePayloads.TryGetValue(type, out Payload payload) && payload.IsActive;

            return active;
        }

        public List<Payload> GetActivePayloads()
        {
            return new List<Payload>(activePayloads);
        }

        // Effect methods that will be called by other systems
        public bool ShouldClusterDataFragments()
        {
            return IsPayloadActive(PayloadType.DataCluster);
        }

        public bool ShouldRevealRandomTilesOnVirus()
        {
            return IsPayloadActive(PayloadType.Phishing);
        }

        public bool ShouldEchoReveal()
        {
            return IsPayloadActive(PayloadType.Echo) && UnityEngine.Random.value < 0.1f;
        }

        public bool ShouldSpreadDamage()
        {
            return IsPayloadActive(PayloadType.DamageOverTime);
        }

        public bool ShouldRevealSimilarTile()
        {
            return IsPayloadActive(PayloadType.WirelessUpload);
        }

        public bool ShouldExpandToolkit()
        {
            return IsPayloadActive(PayloadType.ToolkitExpansion);
        }
        public Payload GetPayloadDetails(PayloadType type)
        {
            availablePayloads.TryGetValue(type, out var payload);
            return payload;
        }
    }
} 