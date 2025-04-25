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
        private readonly IGridService _gridService;
        private readonly IProgressTrackerService _progressTrackerService;

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
            _gridService = gridService ?? throw new ArgumentNullException(nameof(gridService));
            _progressTrackerService = progressTrackerService ?? throw new ArgumentNullException(nameof(progressTrackerService));
        }

        /// <inheritdoc/>
        public void ApplyMutation(MutationType type)
        {
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
            // Reset entropy to 0%
            _entropyService.Decrease(_entropyService.EntropyPercent);
        }

        internal void ApplyMutationEffect(MutationType mutationType)
        {
            ApplyMutation(mutationType);
        }

    }
}
