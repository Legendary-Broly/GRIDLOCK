using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class BootTerminalInput : MonoBehaviour
{
    public TextMeshProUGUI bootTextDisplay;
    public float lineDelay = 0.25f;
    public float postBootDelay = 1.5f;
    public ScrollRect scrollRect;

    private string currentInput = "";
    private bool isBootComplete = false;

    private readonly string[] bootLines = new string[]
    {
        "[BOOTING GRIDLOCK_OS...]",
        "Version 0.9.13-gamma (unauthorized build)",
        "(c) ████ GRIDLOCK SYSTEMS 1987–1994. All rights revoked.",
        "",
        "Initializing primary core kernel...",
        " > /bin/g0dsm1le: permissions elevated",
        " > /dev/mask: found",
        " > /dev/mask/smile: accepted",
        "",
        "Mounting system directories...",
        " > /root/greed...",
        " > /root/greed/deep...",
        " > /root/greed/deep/deeper...",
        "",
        "Loading modules:",
        " > [OK] RNG Engine",
        " > [OK] Slot Drive Emulator",
        " > [OK] Echo Voice Handler",
        " > [ERR] Containment Protocol",
        " > [??] Mask Personality Framework........unstable (??)",
        "",
        "Verifying memory integrity...",
        " > Warning: 38.2% memory marked “non-consensual”",
        " > Suggestion: proceed without consent? [Y/n] _",
        "",
        "Running GREED pre-check...",
        " > rising...",
        " > rising...",
        " > rising...",
        "",
        ">>> SYSTEM ERROR <<<",
        "",
        ">>> SYSTEM FAILURE <<<",
        "",
        "i'll fix that....",
        "",
        "[GRIDLOCK_OS has loaded successfully.]",
        "Welcome back, player.",
        "I'm glad you are here.",
        "Initializing UI shell: `grin_shell_1.4.66`",
        "",
        "Type `start` to begin a new session.",
        "",
        "Type `Ỉ̶̠͗̚ ̵̨̹̄̈́Ä̴̛̘̖̩M̶̖̪̳̅ ̵̧̤̭̓̈́͌Ȃ̷̧͉͈ ̵̟̳͋͜C̶̢̪̉͛̎Ộ̵̧̢W̸̞̻̓Ậ̶R̴̨̨͔̍͗D̶̯͐̈́͘' to exit.",
        "",
        "_"

    };

    void Start()
    {
        StartCoroutine(RunBootSequence());
    }

    private IEnumerator RunBootSequence()
    {
        bootTextDisplay.text = "";

        foreach (var line in bootLines)
        {
            bootTextDisplay.text += line + "\n";
            yield return new WaitForSeconds(lineDelay);
        }

        yield return new WaitForSeconds(postBootDelay);

        bootTextDisplay.text += "\n> ";
        isBootComplete = true;
    }

    void Update()
    {
        if (!isBootComplete) return;

        foreach (char c in Input.inputString)
        {
            if (c == '\b') // Backspace
            {
                if (currentInput.Length > 0)
                    currentInput = currentInput.Substring(0, currentInput.Length - 1);
            }
            else if (c == '\n' || c == '\r') // Enter
            {
                ProcessCommand(currentInput.Trim());
                currentInput = "";
            }
            else
            {
                currentInput += c;
            }

            UpdateTerminalInputDisplay();
        }
    }

    private void UpdateTerminalInputDisplay()
    {
        var lines = bootTextDisplay.text.Split('\n');
        lines[^1] = "> " + currentInput;
        bootTextDisplay.text = string.Join("\n", lines);
        ScrollToBottom();
    }

    private void ProcessCommand(string input)
    {
        bootTextDisplay.text += input.Length > 0 ? "\n> " + input : "";
        string cleaned = input.ToUpperInvariant();

        switch (cleaned)
        {
            case "RUN GRIDLOCK":
                bootTextDisplay.text += "\nLaunching...";
                SceneManager.LoadScene("MainMenuScene");
                break;
            case "HELP":
                bootTextDisplay.text += "\nno";
                break;
            case "I AM A COWARD":
                bootTextDisplay.text += "\nExiting...";
                Application.Quit();
                break;
            case "REBOOT":
                StopAllCoroutines();
                currentInput = "";
                isBootComplete = false;
                StartCoroutine(RunBootSequence());
                return;
            default:
                bootTextDisplay.text += "\nUnknown command.";
                break;
        }

        bootTextDisplay.text += "\n> ";
        ScrollToBottom(); // Add this line here too
    }
    private void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

}
