using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TerminalInputHandler : MonoBehaviour
{
    public TMP_InputField inputField;
    public GameObject inputFieldObject;

    private bool terminalReadyForInput = false;

    public void EnableInput()
    {
        terminalReadyForInput = true;
        inputFieldObject.SetActive(true);
        inputField.ActivateInputField();
    }

    private void Update()
    {
        if (!terminalReadyForInput) return;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            string command = inputField.text.ToUpper().Trim();
            inputField.text = "";
            ProcessCommand(command);
            inputField.ActivateInputField();
        }
    }
    private void ProcessCommand(string command)
    {
        switch (command)
        {
            case "START":
                SceneManager.LoadScene("NewGameplay");
                break;

            case "I AM A COWARD":
                Application.Quit();
                break;

            default:
                Debug.Log($"Unknown command: {command}");
                break;
        }
    }
}