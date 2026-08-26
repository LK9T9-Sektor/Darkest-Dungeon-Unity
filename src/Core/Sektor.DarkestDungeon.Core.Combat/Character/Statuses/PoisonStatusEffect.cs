using Sektor.DarkestDungeon.Core.Combat.Mechanics;

namespace Sektor.DarkestDungeon.Core.Combat.Character.Statuses
{
    /// <summary>Poison damage-over-time status.</summary>
    public class PoisonStatusEffect : DamageOverTimeStatusEffect
    {
        /// <inheritdoc/>
        public override StatusType Type { get { return StatusType.Poison; } }
    }
}