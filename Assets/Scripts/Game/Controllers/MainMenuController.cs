using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("Gameplay"); // Exact scene name
    }

    public void ExitGame()
    {
        Application.Quit();
        // Debug.Log("Game Quit"); // Will show in editor for testing
    }
}
// This script handles the main menu actions, such as starting the game and exiting the application.