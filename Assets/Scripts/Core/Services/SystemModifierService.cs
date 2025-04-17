using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Services
{
    public class SystemModifierService
    {
        private readonly IGameStateService _gameStateService;
        
        // Static cache of bonuses that persists even if service is re-initialized
        private static List<SymbolBonus> _persistentBonuses = new List<SymbolBonus>();

        public SystemModifierService(IGameStateService state)
        {
            _gameStateService = state;
            
            // Restore any existing bonuses from our persistent cache
            if (_persistentBonuses.Count > 0)
            {
                _gameStateService.ActiveDrinkBonuses = new List<SymbolBonus>(_persistentBonuses);
            }
        }

        public void Reset()
        {
            _gameStateService.ActiveDrinkBonuses?.Clear();
            _persistentBonuses.Clear();
        }

        public void ApplyDrinkEffect(DrinkEffectSO drinkEffect)
        {
            if (drinkEffect == null)
            {
                return;
            }

            // Ensure the list exists
            if (_gameStateService.ActiveDrinkBonuses == null)
            {
                _gameStateService.ActiveDrinkBonuses = new List<SymbolBonus>();
            }

            // Store symbol name in lowercase to ensure consistent comparison
            var bonus = new SymbolBonus
            {
                symbolName = drinkEffect.symbolNameToBoost.ToLowerInvariant(),
                weightBonus = drinkEffect.weightBonus
            };

            var activeBonuses = _gameStateService.ActiveDrinkBonuses;
            var existingBonus = activeBonuses
                .FirstOrDefault(b => string.Equals(b.symbolName, bonus.symbolName, System.StringComparison.OrdinalIgnoreCase));

            if (!existingBonus.Equals(default(SymbolBonus)))
            {
                existingBonus.weightBonus += bonus.weightBonus;

                int index = activeBonuses.FindIndex(b => 
                    string.Equals(b.symbolName, bonus.symbolName, System.StringComparison.OrdinalIgnoreCase));
                activeBonuses[index] = existingBonus;
            }
            else
            {
                activeBonuses.Add(bonus);
            }

            // Update persistent cache
            _persistentBonuses = new List<SymbolBonus>(activeBonuses);
        }

        public int GetBonusForSymbol(string symbolName)
        {
            // First try the state service
            var activeBonuses = _gameStateService.ActiveDrinkBonuses;
            
            // If state service has no bonuses, use our persistent cache
            if (activeBonuses == null || activeBonuses.Count == 0)
            {
                if (_persistentBonuses.Count > 0)
                {
                    activeBonuses = _gameStateService.ActiveDrinkBonuses = new List<SymbolBonus>(_persistentBonuses);
                }
                else
                {
                    return 0;
                }
            }

            // Convert input to lowercase for case-insensitive comparison
            string normalizedSymbolName = symbolName.ToLowerInvariant();
            
            // Direct case-insensitive comparison
            foreach (var bonus in activeBonuses)
            {
                string normalizedBonusName = bonus.symbolName.ToLowerInvariant();
                if (normalizedBonusName == normalizedSymbolName)
                {
                    return bonus.weightBonus;
                }
            }
            
            return 0;
        }

        public List<SymbolBonus> GetActiveBonuses()
        {
            // Use persisted bonuses if state service bonuses are empty
            if (_gameStateService.ActiveDrinkBonuses == null || _gameStateService.ActiveDrinkBonuses.Count == 0)
            {
                return _persistentBonuses;
            }
            return _gameStateService.ActiveDrinkBonuses;
        }
    }
}
