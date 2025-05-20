using System;

namespace NewGameplay.Interfaces
{
    public interface ICodeShardTracker
    {
        int CurrentShardCount { get; }
        void AddShard();
        event Action OnShardCountChanged;
    }
}