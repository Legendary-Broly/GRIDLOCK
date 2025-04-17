using UnityEngine;
using UnityEngine.SceneManagement;

public class BarSceneFlow : MonoBehaviour
{
    public void LoadGameplay()
    {
        SceneManager.LoadScene("Gameplay");
    }
}
