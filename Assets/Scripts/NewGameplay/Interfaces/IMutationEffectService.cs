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
    }
}
