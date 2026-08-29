using Sektor.DarkestDungeon.Core.Combat.Character.Components;

namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Battle modifiers of a monster parsed from the battle_modifier block.</summary>
    public sealed class BattleModifier : IBattleModifier
    {
        /// <summary>Initializes a new instance of the <see cref="BattleModifier"/> class.</summary>
        /// <param name="disableStallPenalty">Whether stall penalty is disabled.</param>
        /// <param name="canSurprise">Whether the monster can surprise enemies.</param>
        /// <param name="canBeSurprised">Whether the monster can be surprised.</param>
        /// <param name="alwaysSurprise">Whether the monster always surprises.</param>
        /// <param name="alwaysBeSurprised">Whether the monster is always surprised.</param>
        public BattleModifier(
            bool disableStallPenalty,
            bool canSurprise,
            bool canBeSurprised,
            bool alwaysSurprise,
            bool alwaysBeSurprised)
        {
            DisableStallPenalty = disableStallPenalty;
            CanSurprise = canSurprise;
            CanBeSurprised = canBeSurprised;
            AlwaysSurprise = alwaysSurprise;
            AlwaysBeSurprised = alwaysBeSurprised;
        }

        /// <inheritdoc/>
        public bool DisableStallPenalty { get; }

        /// <inheritdoc/>
        public bool CanSurprise { get; }

        /// <inheritdoc/>
        public bool CanBeSurprised { get; }

        /// <inheritdoc/>
        public bool AlwaysSurprise { get; }

        /// <inheritdoc/>
        public bool AlwaysBeSurprised { get; }

        /// <inheritdoc/>
        public bool IsValidFriendlyTarget { get { return true; } }

        /// <inheritdoc/>
        public bool CanRelieveStressFromKills { get { return true; } }

        /// <inheritdoc/>
        public bool CanRelieveStressFromCrit { get { return true; } }

        /// <inheritdoc/>
        public bool CanBeSummonRank { get { return true; } }

        /// <inheritdoc/>
        public bool CanBeMissed { get { return true; } }

        /// <inheritdoc/>
        public bool? CanBeHit { get { return null; } }

        /// <inheritdoc/>
        public bool? CanBeDamagedDirectly { get { return null; } }
    }
}