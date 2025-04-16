using System.Collections.Generic;

public interface IGameStateService
{
    List<SymbolCard> PlayerHand { get; }
    float CurrentMultiplier { get; }
    float CurrentDoomChance { get; }
    void AddCardToHand(SymbolCard card);
    void AdvanceDraw();
}
