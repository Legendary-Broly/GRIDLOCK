using UnityEngine;

namespace NewGameplay.Interfaces
{
    public interface IPayloadManager
    {
        // Payload State
        bool ShouldRevealSimilarTile();
        bool ShouldRevealRandomTilesOnVirus();
        bool ShouldSpreadDamage();
        
        // Payload Effects
        void ApplyPayloadEffects();
        void ResetPayloadEffects();
        
        // Payload Configuration
        void SetPayloadEnabled(string payloadName, bool enabled);
        bool IsPayloadEnabled(string payloadName);
        
        // Payload Events
        void OnVirusRevealed();
        void OnFragmentRevealed();
        void OnExtractComplete();
    }
} 