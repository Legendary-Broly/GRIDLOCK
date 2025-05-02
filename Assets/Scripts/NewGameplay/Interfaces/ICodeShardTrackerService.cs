using System;

namespace NewGameplay.Interfaces
{
    public interface ICodeShardTracker
    {
        int CurrentShardCount { get; }
        int ShardsRequiredForNextHack { get; }
        void SetShardsRequired(int amount);
        void AddShard();
        bool TrySpendShards();
        event Action OnShardCountChanged;
    }
}