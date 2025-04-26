using NewGameplay.Interfaces;
using UnityEngine;

namespace NewGameplay.Services
{
    public class ScoreService : IScoreService
    {
        private int currentScore = 0;
        private readonly IMutationEffectService mutationEffectService;

        public int CurrentScore => currentScore;

        public ScoreService(IMutationEffectService mutationEffectService)
        {
            this.mutationEffectService = mutationEffectService;
        }

        public ScoreService()
        {
        }

        public int GetCurrentScore()
        {
            return currentScore;
        }

        public void AddScore(int points)
        {
            if (mutationEffectService != null && mutationEffectService.IsMutationActive(MutationType.InfiniteLoop))
            {
                points = Mathf.RoundToInt(points * 0.4f);  // Apply 60% reduction
            }

            currentScore += points;
        }

        public void ResetScore()
        {
            currentScore = 0;
        }
    }
}
