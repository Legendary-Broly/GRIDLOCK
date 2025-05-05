using System;
using System.Collections.Generic;
using UnityEngine;
using NewGameplay.Interfaces;

namespace NewGameplay.Services
{
    public class VirusBlockChecker : IVirusBlockChecker
    {
        private readonly IGridStateService gridStateService;
        private readonly IGridService gridService;
        private readonly Dictionary<string, Func<int, int, bool>> blockingConditions = new Dictionary<string, Func<int, int, bool>>();

        public VirusBlockChecker(IGridStateService gridStateService, IGridService gridService)
        {
            this.gridStateService = gridStateService;
            this.gridService = gridService;
            InitializeDefaultConditions();
        }

        private void InitializeDefaultConditions()
        {
            // Register default blocking conditions
            RegisterBlockingCondition("OutOfBounds", (x, y) => !gridStateService.IsInBounds(x, y));
            RegisterBlockingCondition("AlreadyHasVirus", (x, y) => gridStateService.GetSymbolAt(x, y) == VirusSpreadService.VIRUS_SYMBOL);
            RegisterBlockingCondition("HasDataFragment", (x, y) => gridStateService.GetSymbolAt(x, y) == "DF");
            RegisterBlockingCondition("HasPurge", (x, y) => gridStateService.GetSymbolAt(x, y) == "∆:/run_PURGE.exe");
            RegisterBlockingCondition("AdjacentToLastRevealed", (x, y) => 
            {
                if (gridService == null) return false;
                Vector2Int? last = gridService.GetLastRevealedTile();
                if (!last.HasValue) return false;
                
                return Mathf.Abs(x - last.Value.x) + Mathf.Abs(y - last.Value.y) == 1;
            });
            RegisterBlockingCondition("CanRevealTile", (x, y) => gridService != null && gridService.CanRevealTile(x, y));
        }

        public bool CanVirusSpreadTo(int x, int y)
        {
            foreach (var condition in blockingConditions.Values)
            {
                if (condition(x, y))
                {
                    return false;
                }
            }
            return true;
        }

        public void RegisterBlockingCondition(string conditionName, Func<int, int, bool> condition)
        {
            if (blockingConditions.ContainsKey(conditionName))
            {
                Debug.LogWarning($"[VirusBlockChecker] Condition '{conditionName}' already registered. Overwriting.");
            }
            blockingConditions[conditionName] = condition;
        }

        public void UnregisterBlockingCondition(string conditionName)
        {
            if (blockingConditions.ContainsKey(conditionName))
            {
                blockingConditions.Remove(conditionName);
            }
        }
    }
} 