using System;
using NewGameplay.Interfaces;

namespace NewGameplay.Services
{
    public class CodeShardTrackerService : ICodeShardTracker
    {
        private int currentShardCount;
        private int shardsRequiredForNextHack = 5;

        public int CurrentShardCount => currentShardCount;
        public int ShardsRequiredForNextHack => shardsRequiredForNextHack;

        public event Action OnShardCountChanged;

        public void AddShard()
        {
            currentShardCount++;
            OnShardCountChanged?.Invoke();
        }

        public void SetShardsRequired(int amount)
        {
            shardsRequiredForNextHack = amount;
            OnShardCountChanged?.Invoke();
        }

        public bool TrySpendShards()
        {
            if (currentShardCount >= shardsRequiredForNextHack)
            {
                currentShardCount -= shardsRequiredForNextHack;
                OnShardCountChanged?.Invoke();
                return true;
            }

            return false;
        }
    }
}
