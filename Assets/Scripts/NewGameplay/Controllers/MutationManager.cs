using UnityEngine;
using System.Collections.Generic;
using NewGameplay.ScriptableObjects;
using NewGameplay.Services;
using NewGameplay.Interfaces;

public class MutationManager : MonoBehaviour
{
    [SerializeField] private GameObject mutationPanel;
    [SerializeField] private MutationOption[] mutationOptions;
    [SerializeField] private List<MutationSO> availableMutations;
    [SerializeField] private MutationTracker mutationTracker;  // Drag your MutationTracker GameObject here
    [SerializeField] private MutationEffectService mutationEffectService;

    private List<MutationSO> activeMutations = new List<MutationSO>(); 

    private System.Random random = new System.Random();

    private void Start()
    {
        // Set up mutation options with this manager
        foreach (var option in mutationOptions)
        {
            option.SetManager(this);
        }
    }

    public void ShowMutationPanel()
    {
        mutationPanel.SetActive(true);

        // Get random mutations for each option slot
        var selectedMutations = GetRandomMutations(mutationOptions.Length);
        
        for (int i = 0; i < mutationOptions.Length; i++)
        {
            mutationOptions[i].Setup(selectedMutations[i]);
        }
    }

    private List<MutationSO> GetRandomMutations(int count)
    {
        count = Mathf.Min(count, availableMutations.Count);
        var selectedMutations = new List<MutationSO>();
        var availableIndices = new List<int>();
        
        // Initialize available indices
        for (int i = 0; i < availableMutations.Count; i++)
        {
            availableIndices.Add(i);
        }
        
        // Select random mutations
        for (int i = 0; i < count; i++)
        {
            int randomIndex = random.Next(availableIndices.Count);
            int mutationIndex = availableIndices[randomIndex];
            selectedMutations.Add(availableMutations[mutationIndex]);
            availableIndices.RemoveAt(randomIndex);
        }
        
        return selectedMutations;
    }

    public void SelectMutation(MutationSO mutation)
    {
        ApplyMutationEffect(mutation);
        mutationPanel.SetActive(false);
    }

    public void ApplyMutationEffect(MutationSO selectedMutation)
    {
        activeMutations.Add(selectedMutation);
        mutationEffectService.ApplyMutationEffect(selectedMutation.MutationType);

        mutationTracker.UpdateMutationDisplay(activeMutations.ToArray());
    }

    public void SetMutationEffectService(MutationEffectService service)
    {
        mutationEffectService = service;
    }
}
