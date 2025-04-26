using System;
using UnityEngine;
using NewGameplay.Interfaces;
using NewGameplay.Utility;

namespace NewGameplay.Services
{
    /// <summary>
    /// Implementation of IMutationEffectService that applies various mutation effects to the game state
    /// </summary>
    public class MutationEffectService : IMutationEffectService
    {
        private readonly IEntropyService _entropyService;
        private IGridService _gridService;
        private readonly IProgressTrackerService _progressTrackerService;
        private MutationType? _activeMutation;

        /// <summary>
        /// Initializes a new instance of the MutationEffectService
        /// </summary>
        /// <param name="entropyService">Service for managing entropy mechanics</param>
        /// <param name="gridService">Service for managing the game grid</param>
        /// <param name="progressTrackerService">Service for tracking game progress</param>
        public MutationEffectService(
            IEntropyService entropyService,
            IGridService gridService,
            IProgressTrackerService progressTrackerService)
        {
            _entropyService = entropyService ?? throw new ArgumentNullException(nameof(entropyService));
            _gridService = gridService; // Allow null initially, will be set later
            _progressTrackerService = progressTrackerService ?? throw new ArgumentNullException(nameof(progressTrackerService));
            
            Debug.Log("[MutationEffectService] Constructor called. GridService is " + (gridService != null ? "provided" : "null (will be set later)"));
        }

        /// <summary>
        /// Clears the current active mutation effects
        /// </summary>
        public void ClearCurrentMutation()
        {
            if (_gridService == null)
            {
                Debug.LogError("[MutationEffectService] GridService is null in ClearCurrentMutation!");
                return;
            }
            
            if (_activeMutation.HasValue)
            {
                Debug.Log($"[MutationEffectService] Clearing active mutation: {_activeMutation}");
                
                // Undo specific mutation effects
                switch (_activeMutation.Value)
                {
                    case MutationType.PurgePlus:
                        // Reset virus growth rate
                        _entropyService.ResetVirusGrowthRate();
                        // Disable row/column purge
                        _gridService.DisableRowColumnPurge();
                        break;
                    // Add other mutation-specific cleanup as needed
                }
                
                _activeMutation = null;
            }
        }

        /// <inheritdoc/>
        public void ApplyMutation(MutationType type)
        {
            Debug.Log($"[MutationEffectService] ApplyMutation called with {type} on instance: {this.GetHashCode()}");
            
            if (_gridService == null)
            {
                Debug.LogError("[MutationEffectService] GridService is null in ApplyMutation!");
                return;
            }
            
            // Clear any existing mutation first
            ClearCurrentMutation();
            
            _activeMutation = type;
            switch (type)
            {
                case MutationType.FearOfChange:
                    ApplyFearOfChangeMutation();
                    break;
                case MutationType.OmegaSurge:
                    ApplyOmegaSurgeMutation();
                    break;
                case MutationType.FeedbackLoop:
                    ApplyFeedbackLoopMutation();
                    break;
                case MutationType.Firewall:
                    ApplyFirewallMutation();
                    break;
                case MutationType.Incubation:
                    ApplyIncubationMutation();
                    break;
                case MutationType.Contagion:
                    ApplyContagionMutation();
                    break;
                case MutationType.InfiniteLoop:
                    ApplyInfiniteLoopMutation();
                    break;
                case MutationType.PurgePlus:
                    ApplyPurgePlusMutation();
                    break;
                default:
                    Debug.LogWarning($"Unknown mutation type: {type}");
                    break;
            }
            
            // Trigger an update of grid state to immediately reflect changes
            _gridService.TriggerGridUpdate();
            
            Debug.Log($"[MutationEffectService] After applying {type}, active mutation is: {_activeMutation}");
        }

        /// <inheritdoc/>
        public bool IsMutationActive(MutationType type)
        {
            bool isActive = _activeMutation == type;
            Debug.Log($"[MutationEffectService] IsMutationActive check for {type}: {isActive}, current active mutation: {_activeMutation}");
            return isActive;
        }

        private void ApplyFearOfChangeMutation()
        {
            // Reduce entropy rate by 20%
            _entropyService.ModifyEntropy(-0.2f);
        }

        private void ApplyOmegaSurgeMutation()
        {
            // Increase entropy rate by 30%
            _entropyService.ModifyEntropy(0.3f);
        }

        private void ApplyFeedbackLoopMutation()
        {
            // Increase score by 50% of current score
            int currentScore = _progressTrackerService.CurrentScore;
            _progressTrackerService.ApplyScore(currentScore / 2);
        }

        private void ApplyFirewallMutation()
        {
            // Clear all non-virus tiles
            _gridService.ClearAllExceptViruses();
        }

        private void ApplyIncubationMutation()
        {
            // Increase entropy threshold by 10%
            _entropyService.Increase(10);
        }

        private void ApplyContagionMutation()
        {
            // Spread viruses to adjacent tiles
            _gridService.SpreadVirus();
        }

        private void ApplyInfiniteLoopMutation()
        {
            // Double the current score
            int currentScore = _progressTrackerService.CurrentScore;
            _progressTrackerService.ApplyScore(currentScore);
        }

        private void ApplyPurgePlusMutation()
        {
            _activeMutation = MutationType.PurgePlus;  // Set the active mutation type
            _gridService.EnableRowColumnPurge();          // Extend GridService for row/column purge logic.
            _entropyService.DoubleVirusGrowthRate();      // Extend EntropyService for virus growth rate.
        }

        internal void ApplyMutationEffect(MutationType mutationType)
        {
            Debug.Log($"[MutationEffectService] ApplyMutationEffect called with {mutationType} on instance: {this.GetHashCode()}");
            ApplyMutation(mutationType);
        }
        
        public void SetGridService(IGridService gridService)
        {
            if (gridService == null)
            {
                Debug.LogError("[MutationEffectService] Attempt to set GridService to null!");
                return;
            }
            
            Debug.Log($"[MutationEffectService] Setting GridService: Success");
            _gridService = gridService;
        }
    }
}
