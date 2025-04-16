using UnityEngine;

public class DoomHandler : IDoomHandler
{
    private IGameStateService _stateService;

    public DoomHandler(IGameStateService stateService)
    {
        _stateService = stateService;
    }

    public void IncrementDoom()
    {
        _stateService.AdvanceDraw();
    }

    public bool TryTriggerDoom()
    {
        float chance = _stateService.CurrentDoomChance;
        return Random.value < chance;
    }

    public float GetCurrentDoomChance() => _stateService.CurrentDoomChance;
}
