using NewGameplay.Interfaces;

namespace NewGameplay.Services
{
    public class ScoreService : IScoreService
    {
        private int currentScore;

        public int CurrentScore => currentScore;

        public void AddScore(int points)
        {
            currentScore += points;
        }
    }
} 