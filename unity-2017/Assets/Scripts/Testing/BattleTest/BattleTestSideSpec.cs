using System;
using System.Collections.Generic;

/// <summary>One battle side: up to four slots in rank order (front rank first).</summary>
[Serializable]
public class BattleTestSideSpec
{
    /// <summary>The side's slots, front rank first.</summary>
    public List<BattleTestSlotSpec> Slots = new List<BattleTestSlotSpec>();
}