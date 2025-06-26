using System;
using System.Runtime.CompilerServices;

namespace NewGameplay.Interfaces
{
    public interface ICodeShardTrackerService
    {
        event Action<int> OnCodeShardsChanged;
        
        
        int CurrentCodeShards { get; }
        void AddCodeShards(int amount);
        void RemoveCodeShards(int amount);
        void ResetCodeShards();
    }
}