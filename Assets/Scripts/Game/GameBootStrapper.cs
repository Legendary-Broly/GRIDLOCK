using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Core.Services;

public class GameBootstrapper : MonoBehaviour
{
    public static IGameStateService GameStateService { get; private set; }
    public static CardDrawService CardDrawService { get; private set; }
    public static DoomEffectService DoomEffectService { get; private set; }
    public static DoomHandler DoomHandler { get; private set; }
    public static DoomMeterUI DoomMeterUI { get; private set; }
    public static SystemModifierService SystemModifierService { get; private set; }

    [SerializeField] private GameObject Canvas;

    private void Awake()
    {
        // Load symbol pooll
        var symbolPool = new List<SymbolDataSO>(Resources.LoadAll<SymbolDataSO>("Symbols"));

        // Core services
        GameStateService = new GameStateService(); // ✅ Must come first
        SystemModifierService = new SystemModifierService(GameStateService); // ✅ Corrected reference

        var gameplayUI = FindFirstObjectByType<GameplayUIController>();
        var gridManager = FindFirstObjectByType<GridManager>();

        DoomEffectService = new DoomEffectService(gridManager, gameplayUI);
        DoomHandler = new DoomHandler(GameStateService, DoomEffectService);

        // Card draw service
        CardDrawService = new CardDrawService(symbolPool, GameStateService, DoomHandler, SystemModifierService);

        DoomMeterUI = Canvas.GetComponentInChildren<DoomMeterUI>();
        DoomMeterUI.UpdateDoomMeter(
            GameStateService.CurrentDoomChance,
            GameStateService.CurrentDoomMultiplier,
            GameStateService.CurrentDoomStage
        );

        gameplayUI.Init(); // ✅ Delay until all services hooked up
    }
}
