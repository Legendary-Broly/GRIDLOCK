using System.Collections.Generic;
using NewGameplay.Enums;

namespace NewGameplay.Services
{
    public class SplitFlagManager
    {
        private readonly Dictionary<GridID, bool> hasFlaggedThisTurn = new()
        {
            { GridID.A, false },
            { GridID.B, false }
        };

        /// <summary>
        /// Checks if the player can place a virus flag on the specified grid this turn.
        /// </summary>
        public bool CanFlag(GridID gridId)
        {
            return !hasFlaggedThisTurn[gridId];
        }

        /// <summary>
        /// Marks the flag as used for the given grid this turn.
        /// </summary>
        public void UseFlag(GridID gridId)
        {
            hasFlaggedThisTurn[gridId] = true;
        }

        /// <summary>
        /// Resets both grid flags at the start of a new tile reveal phase.
        /// </summary>
        public void ResetFlags()
        {
            hasFlaggedThisTurn[GridID.A] = false;
            hasFlaggedThisTurn[GridID.B] = false;
        }
    }
}
