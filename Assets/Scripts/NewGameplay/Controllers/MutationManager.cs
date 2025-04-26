using UnityEngine;
using System.Collections.Generic;
using NewGameplay.ScriptableObjects;
using NewGameplay.Services;
using NewGameplay.Interfaces;
using NewGameplay;

public class MutationManager : MonoBehaviour
{
    [SerializeField] private GameObject mutationPanel;
    [SerializeField] private MutationOption[] mutationOptions;
    [SerializeField] private List<MutationSO> availableMutations;
    [SerializeField] private MutationTracker mutationTracker;  // Drag your MutationTracker GameObject here
    private MutationEffectService mutationEffectService;

    // Changed from List to single reference since we only allow one active mutation
    private MutationSO activeMutation = null;

    private System.Random random = new System.Random();

    // Optional: reference to key components to avoid Find calls
    [SerializeField] private GridView gridViewReference;
    [SerializeField] private EntropyTrackerView entropyViewReference;
    [SerializeField] private NewGameplayBootstrapper bootstrapperReference;

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
        Debug.Log($"[MutationManager] Applying mutation: {selectedMutation.MutationType} to service instance: {mutationEffectService?.GetHashCode()}");
        
        // Replace the previous active mutation with the new one
        activeMutation = selectedMutation;
        
        // Apply the new mutation effect
        mutationEffectService.ApplyMutationEffect(selectedMutation.MutationType);
        
        // Update UI to show only the active mutation
        UpdateMutationDisplay();
        
        // Force immediate update of game state UI to reflect mutation effects
        ForceGameStateRefresh();
    }
    
    private void UpdateMutationDisplay()
    {
        // Create an array with one element if we have an active mutation
        if (activeMutation != null)
        {
            mutationTracker.UpdateMutationDisplay(new[] { activeMutation });
        }
        else
        {
            mutationTracker.UpdateMutationDisplay(new MutationSO[0]);
        }
    }

    private void ForceGameStateRefresh()
    {
        // First try direct references if available
        GridView gridView = gridViewReference;
        NewGameplayBootstrapper bootstrapper = bootstrapperReference;
        EntropyTrackerView entropyView = entropyViewReference;
        
        // Fall back to finding objects if needed
        if (gridView == null)
        {
            gridView = Object.FindFirstObjectByType<GridView>();
        }
        
        if (bootstrapper == null && gridView != null)
        {
            bootstrapper = Object.FindFirstObjectByType<NewGameplayBootstrapper>();
        }
        
        if (entropyView == null)
        {
            entropyView = Object.FindFirstObjectByType<EntropyTrackerView>();
        }
        
        // Refresh UI elements
        if (gridView != null && bootstrapper != null)
        {
            Debug.Log("[MutationManager] Forcing immediate grid refresh to apply mutation effect");
            gridView.RefreshGrid(bootstrapper.ExposedGridService);
        }
        
        if (entropyView != null)
        {
            entropyView.Refresh();
        }
    }

    public void SetMutationEffectService(MutationEffectService service)
    {
        Debug.Log("[MutationManager] SetMutationEffectService called. Service instance: " + service.GetHashCode());
        mutationEffectService = service;
    }
}
