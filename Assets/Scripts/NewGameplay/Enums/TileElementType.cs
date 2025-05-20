using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NewGameplay.Enums
{
    public enum TileElementType
    {
        Empty,
        SystemIntegrityIncrease,
        ToolRefresh,
        CodeShard,

        // New tile elements
        Warp,
        FlagPop,
        JunkPile,
        CodeShardPlus,
        SystemIntegrityIncreasePlus,

        // synthetic entries for viruses and data fragments
        Virus = 999,
        DataFragment = 998

    }

}