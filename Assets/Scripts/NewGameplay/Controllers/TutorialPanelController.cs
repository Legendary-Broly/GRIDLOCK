using UnityEngine;

public class TutorialPanelController : MonoBehaviour
{
    [SerializeField] private GameObject tutorialPanel;

    private void Start()
    {
        // Show panel when scene loads
        ShowTutorial();
    }

    public void ShowTutorial()
    {
        tutorialPanel.SetActive(true);
    }

    public void HideTutorial()
    {
        tutorialPanel.SetActive(false);
    }
}
