using System.Collections.Generic;

public interface IGameStateService
{
    List<SymbolCard> PlayerHand { get; }
    float CurrentMultiplier { get; }
    float CurrentDoomChance { get; }
    void AddCardToHand(SymbolCard card);
    void AdvanceDraw();
    int CurrentDoomStage { get; }
    int DoomEffectCount { get; }
    void IncrementDoomEffectCount();
    float CurrentDoomMultiplier { get; }
    List<SymbolBonus> ActiveDrinkBonuses { get; set; }
    int CurrentGridSize { get; set; }
    void AdvanceGridSize();
    int GetCurrentGridSize();

}
