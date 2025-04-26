using System;
using NewGameplay.Utility;

namespace NewGameplay.Interfaces
{
    /// <summary>
    /// Service responsible for applying mutation effects to the game state
    /// </summary>
    public interface IMutationEffectService
    {
        /// <summary>
        /// Applies a mutation effect based on the specified mutation type
        /// </summary>
        /// <param name="type">The type of mutation to apply</param>
        void ApplyMutation(MutationType type);

        /// <summary>
        /// Checks if a specific mutation type is currently active
        /// </summary>
        /// <param name="type">The type of mutation to check</param>
        /// <returns>True if the mutation is active, false otherwise</returns>
        bool IsMutationActive(MutationType type);
        
        /// <summary>
        /// Clears the current active mutation effects
        /// </summary>
        void ClearCurrentMutation();
        
        /// <summary>
        /// Sets the grid service for the mutation service
        /// </summary>
        /// <param name="gridService">The grid service to be set</param>
        void SetGridService(IGridService gridService);
    }
}
