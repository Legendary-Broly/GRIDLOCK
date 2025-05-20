using UnityEngine;
using UnityEngine.SceneManagement;

namespace NewGameplay.Utility
{
    public class StartRunShortcut : MonoBehaviour
    {
        [SerializeField] private string gameplaySceneName = "NewGameplay";

        public void LaunchGame()
        {
            SceneManager.LoadScene(gameplaySceneName);
        }
    }
}
