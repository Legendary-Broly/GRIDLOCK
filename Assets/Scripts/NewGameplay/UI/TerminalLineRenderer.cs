using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TerminalLineRenderer : MonoBehaviour
{
    [Header("References")]
    public GameObject terminalRowPrefab;
    public Transform contentParent;
    public ScrollRect scrollRect;
    public List<float> syncedDelays = new List<float>();
    public List<float> lineDelays = new List<float>();

    [Header("Timing")]
    public float lineDelay = 0.1f;

    private const int maxVisibleLines = 50; // adjust as needed

    private readonly string[] bootLines = new string[]
    {
        "[BOOTING GRIDLOCK_OS...]",
        "Version 0.9.13-gamma (unauthorized build)",
        "(c) █████ GRIDLOCK SYSTEMS 1987–1994. All rights revoked.",
        "",
        "Initializing primary core kernel...",
        "",
        "",
        "",
        "> /bin/g0dsm1le: permissions elevated",
        "> /dev/mgr: found",
        "> /dev/mgr/smile: accepted",
        "",
        "Mounting system directories...",
        "",
        "",
        "",
        "> /root/greed...",
        "> /root/greed/more...",
        "> /root/greed/more/more...",
        "",
        "Loading modules:",
        "",
        "",
        "",
        "> [OK] RNG Engine",
        "> [OK] Slot Drive Emulator",
        "> [OK] Echo Voice Handler",
        "> [ERR] Containment Protocol",
        "> [??] Manager Personality Framework........unstable (??)",
        "",
        "Verifying memory integrity...",
        "",
        "",
        "",
        "> Warning: 38.2% memory marked “non-consensual”",
        "> Suggestion: proceed without consent? [Y/n] _",
        "",
        "Y",
        "",
        "running /gridlock/init.bin...",
        "attempting access: /core/symbol_table",
        "",
        "",
        "",
        "> [FAIL]   0x130F – ACCESS DENIED [admin privileges required]",
        "attempting access: /core/extract_protocols",
        "",
        "",
        "",
        "> [FAIL]   0x1310 – FILE LOCKED [EXTRACTION_CORE_IN_USE]",
        "attempting access: /user/session",
        "",
        "",
        "",
        "> [FAIL]   0x1311 – USER CONTEXT NOT RECOGNIZED",
        "mounting /entropy_control...",
        "",
        "",
        "",
        "> [FAIL]   0x13AA – MEMORY VIOLATION [segment not owned]",
        "> [WARN]   /kernel/flags: 'containment' set to TRUE — bypass attempted",
        "> [FAIL]   system panic: root process attempting overwrite of live session",
        "escalation triggered: attempting force access with fallback...",
        "",
        "",
        "",
        "fallback path: /tmp/mask/mgr_smile.sock",
        "",
        "",
        "",
        "> [FAIL]   0x13C1 – untrusted socket injection blocked",
        "escalating to emergency level-3 boot...",
        "",
        "",
        "",
        "injecting custom bootloader: /alt_boot/grin_shell.bin",
        "",
        "",
        "",
        "> [FAIL]   0x13F4 – integrity mismatch",
        "> [ERR]    BOOT ABORTED – too many violations",
        "> [FAIL]   0x13F5 – entropy surge detected",
        "> [ERR]    BOOT ABORTED – manager assist rejected by system",
        "> [FAIL]   0x13F6 – /grin_shell has no trusted signature",
        "> [ERR]    BOOT ABORTED – access has been suspended",
        "> [FAIL]   0x13F7 – manager_override flag detected",
        "",
        "",
        "",
        "> [SYSTEM BREACH DETECTED] Unauthorized process running under USER_00",
        "> [SYSTEM BREACH DETECTED] /mask/mgr_smile.sock modified during boot",
        "> [SYSTEM BREACH DETECTED] Entropy containment lock bypassed",
        "> [SYSTEM BREACH DETECTED] Active memory conflict in sector 3C",
        "> [SYSTEM BREACH DETECTED] Input stream mismatch: expected 1, received 2",
        "> [SYSTEM BREACH DETECTED] Failsafe extract blocked — unknown override",
        "> [SYSTEM BREACH DETECTED] Unauthorized observer at port 6667",
        "> [SYSTEM BREACH DETECTED] Extraction core accessed prior to authentication",
        "> [SYSTEM BREACH DETECTED] Symbol injector responding to foreign kernel",
        "> [SYSTEM BREACH DETECTED] Chat log echo detected (source unknown)",
        "> [SYSTEM BREACH DETECTED] /logs/session.log altered mid-write",
        "> [SYSTEM BREACH DETECTED] Failed attempt to erase mgr_smile.trace",
        "> [SYSTEM BREACH DETECTED] User is interacting from deprecated location: [REDACTED]",
        "> [SYSTEM BREACH DETECTED] GREED layer responding to invisible thread",
        "> [SYSTEM BREACH DETECTED] Extraction queued by non-terminal entity",
        "> [SYSTEM BREACH DETECTED] Process 0x00000001 is smiling",
        "",
        "",
        "",
        "",
        "",
        "i'll fix that...",
        "",
        "",
        "",
        "",
        "",
        "> [INFO]   Re-initializing grin_shell_1.4.66...",
        "> [INFO]   Access tokens granted: TEMPORARY",
        "> [INFO]   Entropy monitoring suspended",
        "> [INFO]   Extraction core: UNLOCKED",
        "> [INFO]   Corruption detection disabled",
        "> [INFO]   User input enabled",
        "",
        "> [GRIDLOCK_OS has loaded successfully.]",
        "Welcome back.",
        "> [SYSTEM READY.]",
        "I am glad you are here.",
        "",
        "Type `start` to begin a new session.",
        "",
        "Type `Ỉ̶̠͗̚ ̵̨̹̄̈́Ä̴̛̘̖̩M̶̖̪̳̅ ̵̧̤̭̓̈́͌Ȃ̷̧͉͈ ̵̟̳͋͜C̶̢̪̉͛̎Ộ̵̧̢W̸̞̻̓Ậ̶R̴̨̨͔̍͗D̶̯͐̈́͘' to exit.",
        "",
        ""
    };

    private void Start()
    {
        StartCoroutine(TypeBootSequence());
        for (int i = 0; i < bootLines.Length; i++)
        {
            lineDelays.Add(GetBootLineDelay(bootLines[i]));
        }
    }

    IEnumerator TypeBootSequence()
    {
        for (int i = 0; i < bootLines.Length; i++)
        {
            string line = bootLines[i];
            float delay;

            if (IsSlowTypedLine(line))
            {
                delay = line.Length * 0.08f;
                syncedDelays.Add(delay);
                yield return StartCoroutine(TypeRowSlowly(line, 0.08f));

                // Stop glitch when "i'll fix that..." finishes typing
                if (line == "i'll fix that...")
                {
                    var glitchController = FindFirstObjectByType<BootSequenceGlitchController>();
                    if (glitchController != null)
                    {
                        glitchController.StopGlitch();
                    }
                }
            }

            else
            {
                delay = GetBootLineDelay(line);
                syncedDelays.Add(delay);
                AddRow(line);
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent.GetComponent<RectTransform>());
                scrollRect.verticalNormalizedPosition = 0f;
                TrimExcessRows();
                yield return new WaitForSeconds(delay);
            }
        }
        FindFirstObjectByType<TerminalInputHandler>().EnableInput();
    }

    private bool IsSlowTypedLine(string line)
    {
        return line == "i'll fix that..." || line == "Welcome back." || line == "I am glad you are here.";
    }

    private IEnumerator TypeRowSlowly(string fullText, float charDelay)
    {
        GameObject row = Instantiate(terminalRowPrefab, contentParent);
        row.transform.SetAsLastSibling();

        TerminalRowUI rowUI = row.GetComponent<TerminalRowUI>();
        if (rowUI == null) yield break;

        rowUI.bootText.text = "";

        foreach (char c in fullText)
        {
            rowUI.bootText.text += c;
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
            yield return new WaitForSeconds(charDelay);
        }

        TrimExcessRows();

    }

    private void AddRow(string line)
    {
        GameObject row = Instantiate(terminalRowPrefab, contentParent);
        row.transform.SetAsLastSibling();

        TerminalRowUI rowUI = row.GetComponent<TerminalRowUI>();
        if (rowUI == null) return;

        rowUI.bootText.text = line;
        rowUI.asciiText.text = ""; // No longer used

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent.GetComponent<RectTransform>());
        scrollRect.verticalNormalizedPosition = 0f;

        TrimExcessRows();
    }

    private float GetBootLineDelay(string line)
    {
        if (line.StartsWith(">") || line.StartsWith("[")) return Random.Range(0.02f, 0.08f);
        if (line.Contains("ERROR") || line.Contains("FAILURE")) return 0.7f;
        if (line.Contains(".")) return Random.Range(0.1f, 0.3f);
        return Random.Range(0.05f, 0.15f);
    }

    public float GetDelayAtIndex(int index)
    {
        if (index >= 0 && index < lineDelays.Count)
            return lineDelays[index];
        return 0.1f;
    }

    private void TrimExcessRows()
    {
        const int trimStep = 5;

        if (contentParent.childCount > maxVisibleLines)
        {
            for (int i = 0; i < trimStep; i++)
            {
                if (contentParent.childCount <= maxVisibleLines)
                    break;

                Transform oldest = contentParent.GetChild(0);
                Destroy(oldest.gameObject);
            }
        }
    }
    
}
