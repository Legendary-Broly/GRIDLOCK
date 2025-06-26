using System;
using NewGameplay.Interfaces;

namespace NewGameplay.Services
{
    public class CodeShardTrackerService : ICodeShardTrackerService
    {
        private int currentCodeShards;
        private int shardsRequiredForNextHack = 5;

        public int CurrentCodeShards => currentCodeShards;
        public int ShardsRequiredForNextHack => shardsRequiredForNextHack;

        public event Action<int> OnCodeShardsChanged;

        public void AddCodeShards(int amount)
        {
            currentCodeShards += amount;
            OnCodeShardsChanged?.Invoke(currentCodeShards);
        }

        public void RemoveCodeShards(int amount)
        {
            currentCodeShards = Math.Max(0, currentCodeShards - amount);
            OnCodeShardsChanged?.Invoke(currentCodeShards);
        }

        public void ResetCodeShards()
        {
            currentCodeShards = 0;
            OnCodeShardsChanged?.Invoke(currentCodeShards);
        }
    }
}
