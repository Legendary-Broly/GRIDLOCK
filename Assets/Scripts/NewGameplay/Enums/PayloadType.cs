namespace NewGameplay.Enums
{
    public enum PayloadType
    {
        DataCluster,
        //data fragments are more likely to spawn in clusters. IMPLEMENTED
        Phishing,
        //revealing a virus reveals 2 random tiles on the grid as well. IMPLEMENTED
        Echo,
        //revealed tiles have a #% chance to stay revealed next round. IMPLEMENTED
        ToolkitExpansion,
        //adds a fourth tool slot with a random tool to the player's toolkit. IMPLEMENTED
        DamageOverTime,
        //system integrity loss is spread across 3 tile reveals, rather than applying all at once. IMPLEMENTED
        WirelessUpload
        //revealing a tile element has a 50% chance to reveal the closest tile with the same element. IMPLEMENTED
    }
} 