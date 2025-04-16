public interface IDoomHandler
{
    void IncrementDoom();
    bool TryTriggerDoom();
    float GetCurrentDoomChance();
}
