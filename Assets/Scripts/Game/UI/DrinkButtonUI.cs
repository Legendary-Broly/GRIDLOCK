using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DrinkButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Button button;

    private DrinkEffectSO drink;

    public void Setup(DrinkEffectSO drink, System.Action<DrinkEffectSO> onChosen)
    {
        this.drink = drink;
        label.text = drink.displayName;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onChosen.Invoke(drink));
    }
}
