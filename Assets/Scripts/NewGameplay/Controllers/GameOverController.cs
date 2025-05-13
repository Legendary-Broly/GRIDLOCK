using UnityEngine;

namespace NewGameplay.Controllers
{
    public class GameOverController : MonoBehaviour
    {
        [SerializeField] private GameObject gameOverPanel;

        private void Start()
        {
            gameOverPanel.SetActive(false);
        }

        public void ShowGameOver()
        {
            gameOverPanel.SetActive(true);
        }

        public void RestartRun()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}