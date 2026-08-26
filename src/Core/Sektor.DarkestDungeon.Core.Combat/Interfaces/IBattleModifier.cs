namespace Sektor.DarkestDungeon.Core.Combat.Interfaces
{
    /// <summary>Abstraction of battle modifiers for a character.</summary>
    public interface IBattleModifier
    {
        /// <summary>Gets a value indicating whether stall penalty is disabled.</summary>
        bool DisableStallPenalty { get; }

        /// <summary>Gets a value indicating whether the character can surprise enemies.</summary>
        bool CanSurprise { get; }

        /// <summary>Gets a value indicating whether the character can be surprised.</summary>
        bool CanBeSurprised { get; }

        /// <summary>Gets a value indicating whether the character always surprises.</summary>
        bool AlwaysSurprise { get; }

        /// <summary>Gets a value indicating whether the character is always surprised.</summary>
        bool AlwaysBeSurprised { get; }

        /// <summary>Gets a value indicating whether the character is a valid friendly target.</summary>
        bool IsValidFriendlyTarget { get; }

        /// <summary>Gets a value indicating whether stress is relieved from kills.</summary>
        bool CanRelieveStressFromKills { get; }

        /// <summary>Gets a value indicating whether stress is relieved from crits.</summary>
        bool CanRelieveStressFromCrit { get; }

        /// <summary>Gets a value indicating whether the character can be a summon rank.</summary>
        bool CanBeSummonRank { get; }

        /// <summary>Gets a value indicating whether the character can be missed.</summary>
        bool CanBeMissed { get; }

        /// <summary>Gets a value indicating whether the character can be hit.</summary>
        bool? CanBeHit { get; }

        /// <summary>Gets a value indicating whether the character can be damaged directly.</summary>
        bool? CanBeDamagedDirectly { get; }
    }
}
