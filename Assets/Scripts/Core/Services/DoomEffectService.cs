using UnityEngine;
using System.Collections.Generic;

public class DoomEffectService
{
    private GridManager gridManager;
    private GameplayUIController uiController;

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
            case DoomEffectType.LockedTile:
                LockTilePermanently();
                break;
        }
    }

    private DoomEffectType GetRandomEffectForStage(int stage)
    {
        List<DoomEffectType> pool = stage switch
        {
            1 => new() { DoomEffectType.SlotMalfunction, DoomEffectType.SpilledDrink },
            2 => new() { DoomEffectType.ShuffledHand, DoomEffectType.DeadTile },
            3 => new() { DoomEffectType.CardSwap, DoomEffectType.LockedTile },
            _ => new() { DoomEffectType.SlotMalfunction }
        };

        return pool[Random.Range(0, pool.Count)];
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
        uiController.RebuildHandUI(); // ✅ Add this line to reflect the change
    }

    private void ShuffleHand()
    {
        var hand = GameBootstrapper.GameStateService.PlayerHand;

        for (int i = 0; i < hand.Count; i++)
        {
            SymbolCard replacement = new SymbolCard(GameBootstrapper.CardDrawService.GetRandomSymbol()); // 🔁 new method below
            hand[i] = replacement;
        }

        Debug.Log($"[DOOM] Shuffled Hand: All cards replaced.");
        uiController.RebuildHandUI();
    }


    private void MarkTileUnplayable()
    {
        var grid = gridManager.GetTileGrid();
        List<TileSlotController> available = new();

        foreach (var tile in grid)
        {
            if (!tile.IsOccupied())
                available.Add(tile);
        }

        if (available.Count > 0)
        {
            var tile = available[Random.Range(0, available.Count)];
            tile.MarkUnplayable();
            Debug.Log("[DOOM] Dead Tile: One tile is now unplayable.");
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
        }
    }

    private void LockTilePermanently()
    {
        var grid = gridManager.GetTileGrid();
        TileSlotController[,] gridArray = gridManager.GetTileGrid();
        List<TileSlotController> allTiles = new();

        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                allTiles.Add(grid[x, y]);
            }
        }
        
        List<TileSlotController> unlocked = allTiles.FindAll(t => !t.IsLocked());
        if (unlocked.Count > 0)
        {
            var tile = unlocked[Random.Range(0, unlocked.Count)];
            tile.LockPermanently();
            Debug.Log("[DOOM] Locked Tile: A tile is permanently blocked.");
        }
    }
    private void LogHand(string context)
    {
        var hand = GameBootstrapper.GameStateService.PlayerHand;
        Debug.Log($"[HAND] {context} ({hand.Count} cards):");
        foreach (var c in hand)
            Debug.Log($" - {c.Data.symbolName}");
    }
    
}
