namespace NewGameplay.Interfaces
{
    public interface IScoreService
    {
        void AddScore(int points);
        int CurrentScore { get; }
    }
} 