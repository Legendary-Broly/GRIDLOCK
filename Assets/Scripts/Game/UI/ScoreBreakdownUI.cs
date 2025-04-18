using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ScoreBreakdownUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI baseScoreText;
    [SerializeField] private TextMeshProUGUI gridStatesText;
    [SerializeField] private TextMeshProUGUI multiplierText;
    [SerializeField] private TextMeshProUGUI finalScoreText;

    public void ShowBreakdown(int baseScore, string gridStates, float multiplier, int finalScore)
    {
        baseScoreText.text = $"Base Score: {baseScore}";
        gridStatesText.text = $"Grid States: {gridStates}";
        multiplierText.text = $"Doom Multiplier: x{multiplier:0.0}";
        finalScoreText.text = $"Final Score: {finalScore}";

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
     public void LoadBarPhaseScene()
    {
        SceneManager.LoadScene("BarPhase"); // <- Scene name must match exactly!
    }
    
}
