using UnityEngine;

[CreateAssetMenu(menuName = "GRIDLOCK/Game Config")]
public class GameConfigSO : ScriptableObject
{
    [Header("Doom")]
    public float doomStartChance = 0.05f;
    public float doomMaxChance = 0.5f;
    public float doomIncrementPerDraw = 0.05f;

    [Header("Multiplier")]
    public float multiplierStart = 1.0f;
    public float multiplierMax = 4.0f;
    public float multiplierIncrementPerDraw = 0.5f;
}
