using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class BarPhaseManager : MonoBehaviour
{
    [SerializeField] private Transform drinkButtonContainer;
    [SerializeField] private GameObject drinkButtonPrefab;

    private List<DrinkEffectSO> allDrinks;

    private void Start()
    {
        StartCoroutine(DelayedInit());
    }

    private IEnumerator DelayedInit()
    {
        yield return null;

        if (GameBootstrapper.SystemModifierService != null)
        {
            GameBootstrapper.SystemModifierService.Reset();
        }

        allDrinks = Resources.LoadAll<DrinkEffectSO>("DrinkEffects").ToList();

        if (allDrinks == null || allDrinks.Count == 0)
        {
            Debug.LogError("[BAR PHASE] No drinks loaded from Resources/DrinkEffects.");
            yield break;
        }

        ShowRandomDrinks();
    }

    private void ShowRandomDrinks()
    {
        var selected = GetRandomDrinks(3);

        foreach (var drink in selected)
        {
            var button = Instantiate(drinkButtonPrefab, drinkButtonContainer);
            var ui = button.GetComponent<DrinkButtonUI>();
            ui.Setup(drink, OnDrinkChosen);
        }
    }

    private void OnDrinkChosen(DrinkEffectSO chosen)
    {
        GameBootstrapper.SystemModifierService.ApplyDrinkEffect(chosen);
        GameBootstrapper.GameStateService.AdvanceGridSize();

        // Set a flag to reset the game state on return to gameplay
        PlayerPrefs.SetInt("ResetGameState", 1);
        PlayerPrefs.Save();

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
            return new List<DrinkEffectSO>();
        }

        return weighted.OrderBy(x => Random.value).Distinct().Take(count).ToList();
    }
}
