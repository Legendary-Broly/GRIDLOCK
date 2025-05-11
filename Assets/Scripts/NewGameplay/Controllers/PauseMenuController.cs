using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private TutorialPanelController tutorialPanelController;

    public void ShowTutorialPanel()
    {
        tutorialPanelController.ShowTutorial();
        pauseMenuPanel.SetActive(false); // Close the pause menu when tutorial is shown
    }

    private void Start()
    {
        pauseMenuPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    private void TogglePauseMenu()
    {
        pauseMenuPanel.SetActive(!pauseMenuPanel.activeSelf);
    }

    public void OnRestartRun()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);  // Reload current scene
    }

    public void OnQuit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
