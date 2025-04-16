using UnityEngine;

public class DoomHandler : IDoomHandler
{
    private IGameStateService _stateService;
    private DoomEffectService _doomEffectService;

    public DoomHandler(IGameStateService stateService, DoomEffectService doomEffectService)
    {
        _stateService = stateService;
        _doomEffectService = doomEffectService;
    }

    public void IncrementDoom()
    {
        _stateService.AdvanceDraw();
        GameBootstrapper.DoomMeterUI.UpdateDoomMeter(
            _stateService.CurrentDoomChance,
            _stateService.CurrentDoomMultiplier
        );
    }

    public bool TryTriggerDoom()
    {
        float chance = _stateService.CurrentDoomChance;
        bool doomTriggered = Random.value < chance;

        if (doomTriggered)
        {
            int currentStage = _stateService.CurrentDoomStage;
            Debug.Log("DOOM TRIGGERED! Add corrupted card here.");
            _doomEffectService.TriggerDoomEffect(currentStage);
            _stateService.IncrementDoomEffectCount(); // ✅ must be called AFTER effect
        }
        return doomTriggered;
    }

    public float GetCurrentDoomChance() => _stateService.CurrentDoomChance;
}
