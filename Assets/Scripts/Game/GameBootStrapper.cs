using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameBootstrapper : MonoBehaviour
{
    public static IGameStateService GameStateService { get; private set; }
    public static CardDrawService CardDrawService { get; private set; }
    public static DoomEffectService DoomEffectService { get; private set; }
    public static DoomHandler DoomHandler { get; private set; }
    public static DoomMeterUI DoomMeterUI { get; private set; }

    [SerializeField] private GameObject Canvas;

    private void Awake()
    {
        // Load all symbols from Resources/Symbols/
        var symbolPool = new List<SymbolDataSO>(Resources.LoadAll<SymbolDataSO>("Symbols"));

        // Core services
        GameStateService = new GameStateService();
        var gameplayUI = FindFirstObjectByType<GameplayUIController>();
        var gridManager = FindFirstObjectByType<GridManager>();

        DoomEffectService = new DoomEffectService(gridManager, gameplayUI);
        DoomHandler = new DoomHandler(GameStateService, DoomEffectService);

        // Inject with null pool initially (safe DI pattern)
        CardDrawService = new CardDrawService(null, GameStateService, DoomHandler);
        CardDrawService.InitializeSymbolPool(symbolPool);

        // UI
        DoomMeterUI = Canvas.GetComponentInChildren<DoomMeterUI>();
        DoomMeterUI.UpdateDoomMeter(
            GameStateService.CurrentDoomChance,
            GameStateService.CurrentDoomMultiplier,
            GameStateService.CurrentDoomStage
        );

        // Ensure the hand is initialized once everything is ready
        // gameplayUI.Init();
    }

    private void Start()
    {
        FindFirstObjectByType<GameplayUIController>().Init();
    }

}
