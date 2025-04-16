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

    private void Awake()
    {
        // 1. Game state
        GameStateService = new GameStateService(gameConfig);

        // 2. Doom logic
        DoomHandler = new DoomHandler(GameStateService);

        // 3. Card drawing (uses both game state and doom)
        CardDrawService = new CardDrawService(allSymbols, GameStateService, DoomHandler);
    }
}
