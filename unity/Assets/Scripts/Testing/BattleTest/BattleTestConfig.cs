using System;

/// <summary>
/// The full configuration of a battle test: two sides, which side the local player controls, the
/// deterministic seed, the starting torch and the difficulty. Consumed by <see cref="CoreBattleDriver"/>.
/// </summary>
[Serializable]
public class BattleTestConfig
{
    /// <summary>The first side (hero side by default).</summary>
    public BattleTestSideSpec Side1 = new BattleTestSideSpec();

    /// <summary>The second side (the opponent).</summary>
    public BattleTestSideSpec Side2 = new BattleTestSideSpec();

    /// <summary>Whether the local player controls the first side (heroes).</summary>
    public bool PlayerControlsSide1 = true;

    /// <summary>Whether the local player controls the second side (the opponent).</summary>
    public bool PlayerControlsSide2;

    /// <summary>The deterministic battle seed.</summary>
    public int Seed = 7;

    /// <summary>The starting torch amount in [0, 100].</summary>
    public int Torch = 75;

    /// <summary>The quest difficulty used for the battle rules.</summary>
    public int Difficulty = 1;
}