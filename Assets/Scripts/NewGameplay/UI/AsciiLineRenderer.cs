using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AsciiLineRenderer : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI asciiPanelText;
    public GameObject terminalRowPrefab;
    public Transform contentParent;
    public ScrollRect scrollRect;
    public TerminalLineRenderer terminalRenderer;

    [Header("Timing")]
    public float lineDelay = 0.1f;

    private const int maxVisibleLines = 50; // adjust as needed

    private readonly string[] asciilines = new string[]
    {
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
	"                  -==--                                               --==-                        ",
	"                  =-*%%+-=                                          -+#%*-=                        ",
	"                  --+%%%%+--                                      -=#%%%*-=                        ",
	"                  --*%%%%%#=--                                  -=#%%%%%*-=                        ",
	"                  --*%%%%%%%#=-                               ==%%%%%%%%*--                        ",
	"                  =-*%%%%%%%%%#=-                            -*%%%%%%%%%*-=                        ",
	"                  =-*%%%%%%%%%%%*--    ---------------    --*%%%%%%%%%%%*--                        ",
	"                  ==*%%%%%%%%%%%%##====*######**#*#**+====*%%%%%%%%%%%%%*-=                        ",
	"                  =-*%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%*-=                        ",
	"                  --*%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%*--                        ",
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
	"       @%%%%%%%%##%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%##%%%%%%%%%%%@        ",
	"               --#%%%%%%%#=%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%+*%%%%%%%%%+-                     ",
	"               --#%%%%%%%#-=#+=#%%%%%%%%%%%%%%%%%%%%%%%%%%%%#-=*#-*%%%%%%%%%=-                     ",
	"                -*%%%%%%%%%=*+--=#%####%%%%%%%%%%%%%%%####%+---**=#%%%%%%%%*-                      ",
	"                --#%%%%%%%%%#+---+#---=#=----+#-----*%+--=%+---*##%%%%%%%%#--                      ",
	"                 -=#%%%%%%%%%+---+#---=#=----+#-----*%=--=%+--=#%%%%%%%%%#-+                       ",
	"                  -=%%%%%%%%%%#=-+#---=#=----+#-----+%+--=%+-+%%%%%%%%%%#=-                        ",
	"                   -=#%%%%%%%%%%###---=#=----+#-----+%=--=%%%%%%%%%%%%%#=-                         ",
	"                    =-+#%%%%%%%%%%%*++*#=----+#-----*%*+*#%%%%%%%%%%%%+--                          ",
	"                      --*#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%+--                            ",
	"                       --=#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%#+-=+                             ",
	"                         =--*#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%#--=                                ",
	"                            =--=#%%%%%%%%%%%%%%%%%%%%%%%%%%%#+--                                   ",
	"                               ---===+#%%%%%%%%%%%%####+==---=                                     ",
	"                                   ----------------------                                          ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    "                                                                                                   ",
    };

    private void Start()
    {
        StartCoroutine(TypeAsciiSequence());
    }

    private void TrimExcessRows()
    {
        const int maxVisibleLines = 75;
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

    IEnumerator TypeAsciiSequence()
    {
        for (int i = 0; i < asciilines.Length; i++)
        {
            GameObject row = Instantiate(terminalRowPrefab, contentParent);
            row.transform.SetAsLastSibling();

            TerminalRowUI rowUI = row.GetComponent<TerminalRowUI>();
            if (rowUI == null) continue;

            rowUI.bootText.text = "";
            rowUI.asciiText.text = asciilines[i];

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent.GetComponent<RectTransform>());
            scrollRect.verticalNormalizedPosition = 0f;

            TrimExcessRows();

            while (terminalRenderer.syncedDelays.Count <= i)
                yield return null;

            float syncedDelay = terminalRenderer != null && i < terminalRenderer.syncedDelays.Count
                ? terminalRenderer.syncedDelays[i]
                : lineDelay;
                
            yield return new WaitForSeconds(syncedDelay);
        }
    }
}
// This script is responsible for rendering ASCII art lines in a terminal-like UI.
// It uses a coroutine to type out each line with a delay between them, creating a typing effect.