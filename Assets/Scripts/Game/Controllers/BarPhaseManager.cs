using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BarPhaseManager : MonoBehaviour
{
    [SerializeField] private Transform drinkButtonContainer;
    [SerializeField] private GameObject drinkButtonPrefab;

    private List<DrinkEffectSO> allDrinks;

    private void Start()
    {
        Debug.Log("[BAR DEBUG] Start() entered");

        GameBootstrapper.SymbolModifierService.Reset();

        allDrinks = Resources.LoadAll<DrinkEffectSO>("DrinkEffects").ToList();
        Debug.Log($"[BAR DEBUG] Loaded {allDrinks.Count} drinks");

        ShowRandomDrinks();
    }


    private void ShowRandomDrinks()
    {
        var selected = GetRandomDrinks(3);
        Debug.Log($"[BAR DEBUG] Showing {selected.Count} drinks");

        foreach (var drink in selected)
        {
            var button = Instantiate(drinkButtonPrefab, drinkButtonContainer);
            var ui = button.GetComponent<DrinkButtonUI>();
            Debug.Log($"[BAR DEBUG] Button created for: {drink.displayName}");
            ui.Setup(drink, OnDrinkChosen);
        }
    }


    private void OnDrinkChosen(DrinkEffectSO chosen)
    {
        GameBootstrapper.SymbolModifierService.ApplyDrinkEffect(chosen);
        // Transition back to gameplay scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("Gameplay");
    }

    private List<DrinkEffectSO> GetRandomDrinks(int count)
    {
        var weighted = new List<DrinkEffectSO>();

        foreach (var d in allDrinks)
        {
            int weight = d.rarity switch
            {
                DrinkRarity.Common => 60,
                DrinkRarity.Uncommon => 25,
                DrinkRarity.Rare => 10,
                DrinkRarity.VeryRare => 5,
                _ => 1
            };

            for (int i = 0; i < weight; i++)
                weighted.Add(d);
        }

        if (weighted.Count == 0)
        {
            Debug.LogWarning("[BAR DEBUG] Weighted pool is empty.");
            return new List<DrinkEffectSO>();
        }

        // Ensure random drinks picked without crashing
        return weighted.OrderBy(x => Random.value).Distinct().Take(count).ToList();
    }
}
