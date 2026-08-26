using Sektor.DarkestDungeon.Core.Combat.Mechanics;

namespace Sektor.DarkestDungeon.Core.Combat.Character.Statuses
{
    /// <summary>Bleeding damage-over-time status.</summary>
    public class BleedingStatusEffect : DamageOverTimeStatusEffect
    {
        /// <inheritdoc/>
        public override StatusType Type { get { return StatusType.Bleeding; } }
    }
}