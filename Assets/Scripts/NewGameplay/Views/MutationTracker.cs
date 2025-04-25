using UnityEngine;
using TMPro;
using NewGameplay.ScriptableObjects;


public class MutationTracker : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] mutationTexts;  // Assign all 8 text objects in inspector

    public void UpdateMutationDisplay(MutationSO[] activeMutations)
    {
        for (int i = 0; i < mutationTexts.Length; i++)
        {
            if (i < activeMutations.Length && activeMutations[i] != null)
            {
                mutationTexts[i].text = activeMutations[i].MutationName;
            }
            else
            {
                mutationTexts[i].text = "";  // Clear unused slots
            }
        }
    }
}
