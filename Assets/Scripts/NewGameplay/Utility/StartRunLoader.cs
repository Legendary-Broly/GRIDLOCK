using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace NewGameplay.Utility
{
    public class StartRunLoader : MonoBehaviour
    {
        [SerializeField] private GameObject loadingWindow;
        [SerializeField] private float delayBeforeLoad = 2.5f;
        [SerializeField] private string gameplaySceneName = "NewGameplay";

        public void BeginLoadSequence()
        {
            if (loadingWindow != null)
            {
                loadingWindow.SetActive(true);
                StartCoroutine(LoadSceneAfterDelay());
            }
        }

        private IEnumerator LoadSceneAfterDelay()
        {
            yield return new WaitForSeconds(delayBeforeLoad);
            SceneManager.LoadScene(gameplaySceneName);
        }
    }
}
