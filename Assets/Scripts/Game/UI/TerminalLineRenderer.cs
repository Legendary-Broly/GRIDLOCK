using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class TerminalLineRenderer : MonoBehaviour
{
    [Header("References")]
    public GameObject terminalRowPrefab;
    public Transform contentParent;
    public ScrollRect scrollRect;
    public TextMeshProUGUI asciiPanelText;

    [Header("Timing")]
    public float lineDelay = 0.1f;

    private StringBuilder asciiBuilder = new();

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
        "> /dev/mgr: found",
        "> /dev/mgr/smile: accepted",
        ".",
        "Mounting system directories...",
        ".",
        ".",
        ".",
        "> /root/greed...",
        "> /root/greed/more...",
        "> /root/greed/more/more...",
        ".",
        "Loading modules:",
        ".",
        ".",
        ".",
        "> [OK] RNG Engine",
        "> [OK] Slot Drive Emulator",
        "> [OK] Echo Voice Handler",
        "> [ERR] Containment Protocol",
        "> [??] Manager Personality Framework........unstable (??)",
        ".",
        "Verifying memory integrity...",
        ".",
        ".",
        ".",
        "> Warning: 38.2% memory marked “non-consensual”",
        "> Suggestion: proceed without consent? [Y/n] _",
        ".",
        "Y",
        ".",
        "running /gridlock/init.bin...",
        "attempting access: /core/symbol_table",
        ".",
        ".",
        ".",
        "> [FAIL]   0x130F – ACCESS DENIED [admin privileges required]",
        "attempting access: /core/extract_protocols",
        ".",
        ".",
        ".",
        "> [FAIL]   0x1310 – FILE LOCKED [EXTRACTION_CORE_IN_USE]",
        "attempting access: /user/session",
        ".",
        ".",
        ".",
        "> [FAIL]   0x1311 – USER CONTEXT NOT RECOGNIZED",
        "mounting /entropy_control...",
        ".",
        ".",
        ".",
        "> [FAIL]   0x13AA – MEMORY VIOLATION [segment not owned]",
        "> [WARN]   /kernel/flags: 'containment' set to TRUE — bypass attempted",
        "> [FAIL]   system panic: root process attempting overwrite of live session",
        "escalation triggered: attempting force access with fallback...",
        ".",
        ".",
        ".",
        "fallback path: /tmp/mask/mgr_smile.sock",
        ".",
        ".",
        ".",
        "> [FAIL]   0x13C1 – untrusted socket injection blocked",
        "escalating to emergency level-3 boot...",
        ".",
        ".",
        ".",
        "injecting custom bootloader: /alt_boot/grin_shell.bin",
        ".",
        ".",
        ".",
        "> [FAIL]   0x13F4 – integrity mismatch",
        "> [ERR]    BOOT ABORTED – too many violations",
        "> [FAIL]   0x13F5 – entropy surge detected",
        "> [ERR]    BOOT ABORTED – manager assist rejected by system",
        "> [FAIL]   0x13F6 – /grin_shell has no trusted signature",
        "> [ERR]    BOOT ABORTED – access has been suspended",
        "> [FAIL]   0x13F7 – manager_override flag detected",
        ".",
        ".",
        ".",
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
        ".",
        "..",
        "...",
        "....",
        ".....",
        "i'll fix that...",
        ".",
        "..",
        "...",
        "....",
        ".....",
        "> [INFO]   Re-initializing grin_shell_1.4.66...",
        "> [INFO]   Access tokens granted: TEMPORARY",
        "> [INFO]   Entropy monitoring suspended",
        "> [INFO]   Extraction core: UNLOCKED",
        "> [INFO]   Corruption detection disabled",
        "> [INFO]   User input enabled",
        ".",
        "> [GRIDLOCK_OS has loaded successfully.]",
        "Welcome back.",
        "> [SYSTEM READY.",
        "I am glad you are here.",
        ".",
        "Type `start` to begin a new session.",
        ".",
        "Type `Ỉ̶̠͗̚ ̵̨̹̄̈́Ä̴̛̘̖̩M̶̖̪̳̅ ̵̧̤̭̓̈́͌Ȃ̷̧͉͈ ̵̟̳͋͜C̶̢̪̉͛̎Ộ̵̧̢W̸̞̻̓Ậ̶R̴̨̨͔̍͗D̶̯͐̈́͘' to exit.",
        ".",
        "_"
    };

    private readonly string[] asciilines = new string[]
    {
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
	"                  -==--                                             =-==-                          ",
	"                  =-*%%+-=                                          -+#%*-                         ",
	"                  --+%%%%+--                                      -=#%%%*-=                        ",
	"                  --*%%%%%#=--                                  -=#%%%%%*-=                        ",
	"                  --*%%%%%%%#=-                               ==%%%%%%%%*-                         ",
	"                  =-*%%%%%%%%%#=-                            -*%%%%%%%%%*-                         ",
	"                  =-*%%%%%%%%%%%*--    ---------------    --*%%%%%%%%%%%*-                         ",
	"                  ==*%%%%%%%%%%%%##====*######**#*#**+====*%%%%%%%%%%%%%*-                         ",
	"                  =-*%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%*-                         ",
	"                  --*%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%*-                         ",
	"                  --*%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%*-=                        ",
	"       @%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%@        ",
	"     @%#-------+%#--------+%%+--+#=--------#%#---#%%%%%%#=------=#%%+-------+%+--=%%*---%@         ",
	"     %+---------+#---------=%=--+#----------*#---#%%%%%*----------#=--------=%+--=%+--=#%          ",
	"     %=--+%%#---+#---#%%=--=%+--+#---+%%#---*#---#%%%%%*---#%%*---#=--=%%+--=%+--=+---+%           ",
	"     %=--+%%%%%%%#---#%%---=%=--=#---+%%#---*#---#%%%%%*---#%%*---#=--=%%%%%%%+------#%            ",
	"     %=--+%-----+#---------=%+--+#---+%%#---*#---#%%%%%*---#%%*---#=--=%%%%%%%+-----=%%            ",
	"     %=--+%=----+#--------+%%+--=#---+%%#---*#---#%%%%%*---#%%+---#=--=%%%%%%%+--==--*%            ",
	"     %=--+%%#---+#---##---=#%=--+#---*%%#---*#---#%%%%%*---#%%*---#=--=%%#---#+--=#=--+%           ",
	"     %=---------+#---#%*---=%=--=#=---------*#--------+*----------#*---------#+--=%*---*%          ",
	"     @%*-------*%#---#%%#--=%+--+#=-------=#%#--------+%#+------+%%%*------=#%+--+%%*---%@         ",
	"       %%%%%%%%##%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%##%%%%%%%%%%%          ",
	"               --#%%%%%%%#=%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%+*%%%%%%%%%+-                     ",
	"               --#%%%%%%%#-=#+=#%%%%%%%%%%%%%%%%%%%%%%%%%%%%#-=*#-*%%%%%%%%%=-                     ",
	"                -*%%%%%%%%%=*+--=#%####%%%%%%%%%%%%%%%####%+---**=#%%%%%%%%*-                      ",
	"                --#%%%%%%%%%#+---+#---=#=----+#-----*%+--=%+---*##%%%%%%%%#--                      ",
	"                -=#%%%%%%%%%+---+#---=#=----+#-----*%=--=%+--=#%%%%%%%%%#-+                        ",
	"                  -=%%%%%%%%%%#=-+#---=#=----+#-----+%+--=%+-+%%%%%%%%%%#=-                        ",
	"                   -=#%%%%%%%%%%###---=#=----+#-----+%=--=%%%%%%%%%%%%%#=-                         ",
	"                    =-+#%%%%%%%%%%%*++*#=----+#-----*%*+*#%%%%%%%%%%%%+--                          ",
	"                      --*#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%+--                            ",
	"                       --=#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%#+-=+                             ",
	"                         =--*#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%#--=                                ",
	"                            =--=#%%%%%%%%%%%%%%%%%%%%%%%%%%%#+--                                   ",
	"                               ---===+#%%%%%%%%%%%%####+==---=                                     ",
	"                                   ----------------------                                          ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    ".                                                                                                  ",
    };

    private void Start()
    {
        asciiPanelText.text = ""; // Clear at start
        StartCoroutine(TypeBootSequenceSynced());
    }

    private IEnumerator TypeBootSequenceSynced()
    {
        int asciiIndex = 0;
        int bootIndex = 0;
        string asciiProgress = "";

        while (bootIndex < bootLines.Length)
        {
            string currentLine = bootLines[bootIndex];

            if (asciiIndex < asciilines.Length)
            {
                asciiProgress += asciilines[asciiIndex] + "\n";
                asciiPanelText.text = asciiProgress;
                asciiIndex++;
            }

            if (IsSlowTypedLine(currentLine))
                yield return StartCoroutine(TypeRowSlowly(currentLine));
            else
            {
                AddRow(currentLine);
                yield return new WaitForSeconds(GetBootLineDelay(currentLine));
            }

            // Stop syncing once we hit the last line before "i'll fix that..."
            if (currentLine.Trim() == "i’ll fix that...")
                break;

            bootIndex++;
        }

        // Continue boot text without syncing from here
        for (int i = bootIndex + 1; i < bootLines.Length; i++)
        {
            string line = bootLines[i];
            if (IsSlowTypedLine(line))
                yield return StartCoroutine(TypeRowSlowly(line));
            else
            {
                AddRow(line);
                yield return new WaitForSeconds(GetBootLineDelay(line));
            }
        }
    }

    private bool IsSlowTypedLine(string line)
    {
        return line == "i’ll fix that..."
            || line == "Welcome back."
            || line == "> [SYSTEM READY."
            || line == "I am glad you are here.";
    }

    private IEnumerator TypeRowSlowly(string fullText)
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
            yield return new WaitForSeconds(0.08f);
        }
    }

    private void AddRow(string line)
    {
        GameObject row = Instantiate(terminalRowPrefab, contentParent);
        row.transform.SetAsLastSibling();

        TerminalRowUI rowUI = row.GetComponent<TerminalRowUI>();
        if (rowUI == null) return;

        rowUI.bootText.text = line;
        rowUI.asciiText.text = ""; // deprecated

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent.GetComponent<RectTransform>());
        scrollRect.verticalNormalizedPosition = 0f;
    }

    private float GetBootLineDelay(string line)
    {
        if (line.StartsWith(">") || line.StartsWith("[")) return Random.Range(0.02f, 0.08f);
        if (line.Contains("ERROR") || line.Contains("FAILURE")) return 0.7f;
        if (line.Contains(".")) return Random.Range(0.1f, 0.3f);
        return Random.Range(0.05f, 0.15f);
    }
}
