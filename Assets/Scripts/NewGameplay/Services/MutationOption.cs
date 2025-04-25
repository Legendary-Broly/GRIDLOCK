using UnityEngine;
using TMPro;
using UnityEngine.UI;
using NewGameplay.ScriptableObjects;

public class MutationOption : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI mutationName;
    [SerializeField] private TextMeshProUGUI mutationDescription;
    [SerializeField] private Image mutationIcon;
    [SerializeField] private Button acceptButton;

    private MutationSO currentMutation;
    private MutationManager mutationManager;

    public void Setup(MutationSO mutation)
    {
        currentMutation = mutation;
        mutationName.text = mutation.MutationName;
        mutationDescription.text = mutation.MutationDescription;
        mutationIcon.sprite = mutation.MutationIcon;

        acceptButton.onClick.RemoveAllListeners();
        acceptButton.onClick.AddListener(() => mutationManager.SelectMutation(currentMutation));
    }

    public void SetManager(MutationManager manager)
    {
        mutationManager = manager;
    }
}
