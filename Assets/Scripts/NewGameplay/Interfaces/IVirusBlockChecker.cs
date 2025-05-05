using UnityEngine;
using System;

namespace NewGameplay.Interfaces
{
    /// <summary>
    /// Interface for checking virus spread blocking conditions
    /// </summary>
    public interface IVirusBlockChecker
    {
        /// <summary>
        /// Checks if a virus can spread to the specified position
        /// </summary>
        bool CanVirusSpreadTo(int x, int y);

        /// <summary>
        /// Registers a new blocking condition
        /// </summary>
        void RegisterBlockingCondition(string conditionName, Func<int, int, bool> condition);

        /// <summary>
        /// Unregisters a blocking condition
        /// </summary>
        void UnregisterBlockingCondition(string conditionName);
    }
} 