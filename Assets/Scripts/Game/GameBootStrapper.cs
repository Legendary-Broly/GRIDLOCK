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
    public static GridManager GridManager { get; private set; }

    [SerializeField] private GameObject Canvas;

    private void Awake()
    {
        // Load symbol pool
        var symbolPool = new List<SymbolDataSO>(Resources.LoadAll<SymbolDataSO>("Symbols"));

        // Core services - use the singleton instance
        var gameStateInstance = FindFirstObjectByType<GameStateService>();
        if (gameStateInstance == null)
        {
            Debug.LogError("GameStateService not found in scene. Make sure it exists in the scene.");
            return;
        }
        
        GameStateService = gameStateInstance; // Assign the instance to our static property
        SystemModifierService = new SystemModifierService(GameStateService);

        var gameplayUI = FindFirstObjectByType<GameplayUIController>();
        if (gameplayUI == null)
        {
            Debug.LogError("GameplayUIController not found in scene.");
            return;
        }
        
        GridManager = FindFirstObjectByType<GridManager>();
        if (GridManager == null)
        {
            Debug.LogError("GridManager not found in scene.");
            return;
        }

        DoomEffectService = new DoomEffectService(GridManager, gameplayUI);
        DoomHandler = new DoomHandler(GameStateService, DoomEffectService);

        // Card draw service
        CardDrawService = new CardDrawService(symbolPool, GameStateService, DoomHandler, SystemModifierService);

        if (Canvas != null)
        {
            DoomMeterUI = Canvas.GetComponentInChildren<DoomMeterUI>();
            if (DoomMeterUI != null)
            {
                DoomMeterUI.UpdateDoomMeter(
                    GameStateService.CurrentDoomChance,
                    GameStateService.CurrentDoomMultiplier,
                    GameStateService.CurrentDoomStage
                );
            }
            else
            {
                Debug.LogError("DoomMeterUI component not found on Canvas");
            }
        }
        else
        {
            Debug.LogError("Canvas not assigned in GameBootstrapper");
        }

        // Only initialize UI if everything is properly set up
        if (GameStateService != null && CardDrawService != null && GridManager != null)
        {
            gameplayUI.Init(); // Initialize UI with all services available
        }
    }
}
