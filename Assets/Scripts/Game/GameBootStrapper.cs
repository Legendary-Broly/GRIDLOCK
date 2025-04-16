using System.Collections.Generic;
using UnityEngine;

public class GameBootstrapper : MonoBehaviour
{
    [Header("Config References")]
    [SerializeField] private GameConfigSO gameConfig;
    [SerializeField] private List<SymbolDataSO> allSymbols;

    public static IGameStateService GameStateService { get; private set; }
    public static IDoomHandler DoomHandler { get; private set; }
    public static ICardDrawService CardDrawService { get; private set; }
    public static DoomMeterUI DoomMeterUI { get; private set; }

    public static void Init(GameObject root)
    {
        // After instantiating or finding the UI prefab:
        DoomMeterUI = root.GetComponentInChildren<DoomMeterUI>();
    }

    private void Awake()
    {
        // 1. Game state
        GameStateService = new GameStateService(gameConfig);
        var gridManager = GameObject.FindFirstObjectByType<GridManager>();
        var gameplayUIController = GameObject.FindFirstObjectByType<GameplayUIController>();
        

        // 2. Doom logic
        var doomEffectService = new DoomEffectService(gridManager, gameplayUIController);
        DoomHandler = new DoomHandler(GameStateService, doomEffectService); // ✅ Assign to static

        // 3. Card drawing (uses both game state and doom)
        CardDrawService = new CardDrawService(allSymbols, GameStateService, DoomHandler);
    }
}
