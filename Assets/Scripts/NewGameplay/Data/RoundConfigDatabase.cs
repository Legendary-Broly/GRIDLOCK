using System.Collections.Generic;
using NewGameplay.ScriptableObjects;
using UnityEngine;

[CreateAssetMenu(fileName = "RoundConfigDatabase", menuName = "Gridlock/RoundConfigDatabase")]
public class RoundConfigDatabase : ScriptableObject
{
    public List<RoundConfigSO> roundConfigs;

    public RoundConfigSO GetConfigForRound(int round)
    {
        return roundConfigs.Find(r => r.roundNumber == round);
    }
}