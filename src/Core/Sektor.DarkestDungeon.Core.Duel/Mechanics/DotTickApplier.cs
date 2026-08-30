using Sektor.DarkestDungeon.Core.Combat.Character.Statuses;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;

namespace Sektor.DarkestDungeon.Core.Duel.Mechanics
{
    /// <summary>
    /// Applies the damage-over-time ticks (bleed and poison) at the start of a unit's turn.
    /// The tick deals the combined <see cref="DamageOverTimeStatusEffect.CurrentTickDamage"/>;
    /// death from the tick is handled separately by the caller.
    /// </summary>
    public class DotTickApplier
    {
        private readonly IBattleEvents events;

        /// <summary>Initializes a new instance of the <see cref="DotTickApplier"/> class.</summary>
        /// <param name="events">The battle events sink for popups/overlays.</param>
        public DotTickApplier(IBattleEvents events)
        {
            this.events = events;
        }

        /// <summary>Applies the bleed and poison ticks of the unit.</summary>
        /// <param name="unit">The acting unit.</param>
        public void Apply(ICombatUnit unit)
        {
            ApplyStatusTick(unit, StatusType.Bleeding);
            ApplyStatusTick(unit, StatusType.Poison);
        }

        private void ApplyStatusTick(ICombatUnit unit, StatusType statusType)
        {
            var dot = (DamageOverTimeStatusEffect)unit.Character.GetStatusEffect(statusType);
            if (!dot.IsApplied)
                return;

            int tickDamage = dot.CurrentTickDamage;
            unit.Character.TakeDamage(tickDamage);
            events.ShowPopup(unit, PopupType.Damage, tickDamage.ToString());
            events.UpdateOverlay(unit);
        }
    }
}