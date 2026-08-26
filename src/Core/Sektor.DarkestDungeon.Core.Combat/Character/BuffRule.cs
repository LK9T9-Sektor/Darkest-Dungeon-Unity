namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Buff activation rules.</summary>
    public enum BuffRule : byte
    {
        Always, Size, LightBelow, LightAbove,
        HpBelow, HpAbove, InRank,
        StressAbove, StressBelow,
        Skill, Afflicted, Virtued,
        Melee, Ranged, FirstRound,
        Status, EnemyType, DeathsDoor,
        InCamp, InDungeon, WalkBack,
        InActivity, InCorridor,
        Riposting, InMode
    }
}
