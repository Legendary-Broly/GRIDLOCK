// Assets/Scripts/NewGameplay/Services/PurgeEffectService.cs

using NewGameplay.Interfaces;
using UnityEngine;

namespace NewGameplay.Services
{
    public class PurgeEffectService : IPurgeEffectService
    {
        private readonly IGridService gridService;

        public PurgeEffectService(GridStateService gridStateService, IGridService gridService)
        {
            this.gridService = gridService;
        }

        public bool HandlePurgeEffect(int x, int y)
        {
            if (!gridService.IsInBounds(x, y)) return false;

            var symbolAtTile = gridService.GetSymbolAt(x, y);
            if (symbolAtTile == "X") // Virus
            {
                gridService.SetSymbol(x, y, ""); // Clear the virus
                Debug.Log($"[PurgeEffectService] Virus purged at ({x},{y})");
                return true;
            }
            return false;
        }
    }
}
