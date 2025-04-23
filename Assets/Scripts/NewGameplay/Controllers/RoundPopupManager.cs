using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RoundPopupController : MonoBehaviour
{
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject popupWindow;
    [SerializeField] private TextMeshProUGUI popupText;

    private bool isContinueButtonClicked = false; // Flag to prevent multiple clicks

    public System.Action onContinue;

    private void Awake()
    {
        popupPanel.SetActive(false);
        continueButton.onClick.AddListener(OnContinueClicked);
    }

    public void ShowPopup(int roundNumber)
    {
        popupWindow.SetActive(true);
        popupText.text = 
            $"> USER/EXTRACTION COMPLETE\n" +
            $"    > INITIALIZING USER/EXTRACTION/{roundNumber}\n" +
            $"        > COMPLETE\n\n" +
            $"> USER/EXTRACTION/{roundNumber} READY\n\n" +
            "[mgr_smile]: Excellent work";
        isContinueButtonClicked = false; // Reset flag when popup is shown
    }

    public void OnContinueClicked()
    {
        if (isContinueButtonClicked) return; // Prevent multiple invocations

        isContinueButtonClicked = true; // Set flag to true to block further clicks
        popupPanel.SetActive(false);
        onContinue?.Invoke();
    }

}