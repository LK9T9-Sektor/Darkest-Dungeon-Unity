using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Party;

namespace Sektor.DarkestDungeon.Core.Duel.Mechanics
{
    /// <summary>
    /// Executes a heart attack when a hero's stress reaches 200. A hero already at death's door is
    /// marked for death and dies; otherwise the hero takes 100% of max health damage, stress drops
    /// to 75% and the hero enters death's door (the death check runs afterwards).
    /// </summary>
    public class HeartAttackHandler
    {
        private readonly DeathCheck deathCheck;
        private readonly DuelBattleEvents events;

        /// <summary>Initializes a new instance of the <see cref="HeartAttackHandler"/> class.</summary>
        /// <param name="deathCheck">The death check used after the heart attack damage.</param>
        /// <param name="events">The duel battle events sink.</param>
        public HeartAttackHandler(DeathCheck deathCheck, DuelBattleEvents events)
        {
            this.deathCheck = deathCheck;
            this.events = events;
        }

        /// <summary>Applies the heart attack to the hero.</summary>
        /// <param name="unit">The hero suffering the heart attack.</param>
        public void Apply(ICombatUnit unit)
        {
            var hero = unit.Character as Hero;
            if (hero == null)
                return;

            if (hero.AtDeathsDoor)
            {
                unit.CombatInfo.MarkedForDeath = true;
                deathCheck.Check();
                events.ShowPopup(unit, PopupType.HeartAttack, "100");
                events.ShowPopup(unit, PopupType.DeathBlow);
                return;
            }

            hero.TakeDamagePercent(1.0f);
            hero.Stress.ValueRatio = 0.75f;
            events.ShowPopup(unit, PopupType.HeartAttack, "100");
            deathCheck.Check();
        }
    }
}