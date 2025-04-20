using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TerminalLineRenderer : MonoBehaviour
{
    public GameObject textLinePrefab; // A prefab with just a TMPUGUI Text
    public Transform contentParent;   // Assign Content (scroll container)
    public ScrollRect scrollRect;

    public float lineDelay = 0.1f;

    private readonly string[] bootLines = new string[]
    {
        "[BOOTING GRIDLOCK_OS...]",
        "Version 0.9.13-gamma (unauthorized build)",
        "(c) █████ GRIDLOCK SYSTEMS 1987–1994. All rights revoked.",
        ".",
        "Initializing primary core kernel...",
        ".",
        ".",
        ".",
        "> /bin/g0dsm1le: permissions elevated",
        "> /dev/mask: found",
        "> /dev/mask/smile: accepted",
        ".",
        "Mounting system directories...",
        ".",
        ".",
        ".",
        "> /root/greed...",
        "> /root/greed/deep...",
        "> /root/greed/deep/deeper...",
        ".",
        "Loading modules:",
        ".",
        ".",
        ".",
        "> [OK] RNG Engine",
        "> [OK] Slot Drive Emulator",
        "> [OK] Echo Voice Handler",
        "> [ERR] Containment Protocol",
        "> [??] Mask Personality Framework........unstable (??)",
        ".",
        "Verifying memory integrity...",
        ".",
        ".",
        ".",
        "> Warning: 38.2% memory marked “non-consensual”",
        "> Suggestion: proceed without consent? [Y/n] _",
        ".",
        "Running GREED pre-check...",
        ".",
        "> rising...",
        ".",
        "> rising...",
        ".",
        "> rising...",
        ".",
        ">>> SYSTEM ERROR <<<",
        ".",
        ".",
        ".",
        ">>> SYSTEM FAILURE <<<",
        ".",
        "i’ll fix that...",
        ".",
        ".",
        ".",
        "[GRIDLOCK_OS has loaded successfully.]",
        "Welcome back, player.",
        "I'm glad you are here.",
        ".",
        "Initializing UI shell: `grin_shell_1.4.66`",
        ".",
        ".",
        ".",
        "Type `start` to begin a new session.",
        ".",
        "Type `Ỉ̶̠͗̚ ̵̨̹̄̈́Ä̴̛̘̖̩M̶̖̪̳̅ ̵̧̤̭̓̈́͌Ȃ̷̧͉͈ ̵̟̳͋͜C̶̢̪̉͛̎Ộ̵̧̢W̸̞̻̓Ậ̶R̴̨̨͔̍͗D̶̯͐̈́͘' to exit.",
        ".",
        "_"
    };

    void Start()
    {
        StartCoroutine(TypeBootSequence());
    }

    IEnumerator TypeBootSequence()
    {
        foreach (string line in bootLines)
        {
            // If line should be typed character-by-character
            if (IsSlowTypedLine(line))
            {
                yield return StartCoroutine(TypeLineSlowly(line));
            }
            else
            {
                AddLine(line);

                float delay = GetBootLineDelay(line);
                yield return new WaitForSeconds(delay);
            }
        }
    }
    private bool IsSlowTypedLine(string line)
    {
        return line == "i’ll fix that..."
            || line == "Welcome back, player."
            || line == "I'm glad you are here.";
    }
    private IEnumerator TypeLineSlowly(string fullText)
    {
        GameObject newLine = Instantiate(textLinePrefab, contentParent);
        TextMeshProUGUI tmp = newLine.GetComponent<TextMeshProUGUI>();
        tmp.text = "";

        newLine.transform.SetAsLastSibling();

        float charDelay = 0.08f; // Adjust typing speed here

        foreach (char c in fullText)
        {
            tmp.text += c;
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
            yield return new WaitForSeconds(charDelay);
        }
    }
    private float GetBootLineDelay(string line)
    {
        if (line.StartsWith(">") || line.StartsWith("["))
            return Random.Range(0.02f, 0.08f);
        if (line.Contains("ERROR") || line.Contains("FAILURE"))
            return 0.7f;
        if (line.Contains("."))
            return Random.Range(0.1f, 0.3f);
        return Random.Range(0.05f, 0.15f);
    }

    private void AddLine(string text)
    {
        GameObject newLine = Instantiate(textLinePrefab, contentParent);

        // Ensure proper layout stacking
        newLine.transform.SetAsLastSibling();

        // Assign text
        TextMeshProUGUI tmp = newLine.GetComponent<TextMeshProUGUI>();
        tmp.text = text;

        // Enforce preferred height per line
        LayoutElement layout = newLine.GetComponent<LayoutElement>();
        if (layout != null)
        {
            layout.preferredHeight = 20; // force-tight height
        }

        // Trigger layout rebuild and scroll
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

}
