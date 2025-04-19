using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class DoomEffectService
{
    private GridManager gridManager;
    private GameplayUIController uiController;
    [SerializeField] private Image symbolRenderer;
    private bool isLocked = false;
    private SymbolCard currentCard;

    public bool HasSymbol()
    {
        return currentCard != null;
    }

    public DoomEffectService(GridManager grid, GameplayUIController ui)
    {
        gridManager = grid;
        uiController = ui;
    }

    public void TriggerDoomEffect(int doomStage)
    {
        DoomEffectType effect = GetRandomEffectForStage(doomStage);
        Debug.Log($"[DOOM] Stage {doomStage} effect triggered: {effect}");

        switch (effect)
        {
            case DoomEffectType.SlotMalfunction:
                RemoveRandomTileModifier();
                break;
            case DoomEffectType.SpilledDrink:
                DiscardRandomHandCard();
                break;
            case DoomEffectType.ShuffledHand:
                ShuffleHand();
                break;
            case DoomEffectType.DeadTile:
                MarkTileUnplayable();
                break;
            case DoomEffectType.CardSwap:
                SwapTwoPlacedCards();
                break;
        }
    }

    private DoomEffectType GetRandomEffectForStage(int stage)
    {
        List<DoomEffectType> pool = stage switch
        {
            1 => new() { DoomEffectType.SlotMalfunction, DoomEffectType.SpilledDrink },
            2 => new() { DoomEffectType.ShuffledHand, DoomEffectType.CardSwap },
            3 => new() { DoomEffectType.DeadTile },
            _ => new() { DoomEffectType.SlotMalfunction }
        };

        return pool[Random.Range(0, pool.Count)];
    }
    private bool isUnplayable = false;

    public void MarkUnplayable()
    {
        isUnplayable = true;

        if (symbolRenderer != null)
            symbolRenderer.color = Color.gray;
        //else
            //Debug.LogWarning($"[TileSlot] symbolRenderer not assigned on: {gameObject.name}");
    }

    public bool IsPlayable()
    {
        return !isLocked && !isUnplayable && !HasSymbol();
    }

    private void RemoveRandomTileModifier()
    {
        TileSlotController[,] grid = gridManager.GetTileGrid();
        List<TileSlotController> allTiles = new();

        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                allTiles.Add(grid[x, y]);
            }
        }

        var moddedTiles = allTiles.FindAll(t => t.ActiveModifier != null);

        if (moddedTiles.Count > 0)
        {
            var tile = moddedTiles[Random.Range(0, moddedTiles.Count)];
            tile.AssignModifier(null);
            Debug.Log($"[DOOM] Slot Malfunction: Removed bonus from tile.");
            uiController.ShowDoomEffect("Slot Malfunction - A bonus tile was removed.");
        }
    
    }

    private void DiscardRandomHandCard()
    {
        var hand = GameBootstrapper.GameStateService.PlayerHand;
        if (hand.Count == 0) return;

        var toRemove = hand[Random.Range(0, hand.Count)];
        hand.Remove(toRemove);
        LogHand("After SpilledDrink");

        Debug.Log($"[DOOM] Spilled Drink: Discarded {toRemove.Data.symbolName} from hand.");
        uiController.RebuildHandUI();
        uiController.ShowDoomEffect($"Spilled Drink - A {toRemove.Data.symbolName} card was discarded.");
    }

    private void ShuffleHand()
    {
        var hand = GameBootstrapper.GameStateService.PlayerHand;

        for (int i = 0; i < hand.Count; i++)
        {
            SymbolCard replacement = new SymbolCard(GameBootstrapper.CardDrawService.GetWeightedSymbol());
            hand[i] = replacement;
        }

        Debug.Log($"[DOOM] Shuffled Hand: All cards replaced.");
        uiController.RebuildHandUI();
        uiController.ShowDoomEffect("Shuffled Hand - All cards in your hand were replaced.");
    }

    private void MarkTileUnplayable()
    {
        var grid = gridManager.GetTileGrid();
        List<TileSlotController> allTiles = new();

        foreach (var tile in grid)
        {
            if (tile != null)
                allTiles.Add(tile); // ✅ Include all tiles regardless of occupation
        }

        if (allTiles.Count > 0)
        {
            var tile = allTiles[Random.Range(0, allTiles.Count)];
            tile.MarkTileUnplayable();
            Debug.Log("[DOOM] Dead Tile: One tile is now unplayable.");
            uiController.ShowDoomEffect("Dead Tile - A grid tile has become permanently unplayable.");
        }
    }


    private void SwapTwoPlacedCards()
    {
        var grid = gridManager.GetTileGrid();
        List<TileSlotController> placed = new();

        foreach (var tile in grid)
        {
            if (tile.IsOccupied())
                placed.Add(tile);
        }

        if (placed.Count >= 2)
        {
            var a = placed[Random.Range(0, placed.Count)];
            TileSlotController b;
            do {
                b = placed[Random.Range(0, placed.Count)];
            } while (b == a);

            SymbolCard temp = a.CurrentCard;
            a.ForceSetCard(b.CurrentCard);
            b.ForceSetCard(temp);

            a.RefreshVisuals();
            b.RefreshVisuals();
            Debug.Log("[DOOM] Card Swap: Two placed cards were swapped.");
            uiController.ShowDoomEffect("Card Swap - Two cards on the grid have changed positions.");
        }
    }

    private void LogHand(string context)
    {
        // Method kept for potential future use but logs removed
    }
    
}
