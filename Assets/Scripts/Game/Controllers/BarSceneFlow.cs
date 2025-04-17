using UnityEngine;
using UnityEngine.SceneManagement;

public class BarSceneFlow : MonoBehaviour
{
    public void LoadGameplay()
    {
        // Set a flag in PlayerPrefs that indicates we're returning from the bar phase
        // This will be checked in the Gameplay scene to reset the state
        PlayerPrefs.SetInt("ResetGameState", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Gameplay");
    }
}
