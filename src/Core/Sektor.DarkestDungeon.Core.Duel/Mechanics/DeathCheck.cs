using System.Collections.Generic;
using System.Linq;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Content;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Party;

namespace Sektor.DarkestDungeon.Core.Duel.Mechanics
{
    /// <summary>
    /// Detects newly dead units (health at or below zero) and applies the party stress effect
    /// ("Stress 2") to surviving heroes when a hero dies. Death from DoT ticks and skill damage is
    /// reported through the same check.
    /// </summary>
    public class DeathCheck
    {
        private readonly FormationParty heroParty;
        private readonly FormationParty monsterParty;
        private readonly IDuelContent content;
        private readonly DuelBattleContext context;

        /// <summary>Initializes a new instance of the <see cref="DeathCheck"/> class.</summary>
        /// <param name="heroParty">The hero party.</param>
        /// <param name="monsterParty">The monster party.</param>
        /// <param name="content">The content source (for the stress effect).</param>
        /// <param name="context">The duel battle context.</param>
        public DeathCheck(FormationParty heroParty, FormationParty monsterParty, IDuelContent content, DuelBattleContext context)
        {
            this.heroParty = heroParty;
            this.monsterParty = monsterParty;
            this.content = content;
            this.context = context;
        }

        /// <summary>Marks newly dead units and applies the death stress to surviving heroes.</summary>
        public void Check()
        {
            var newlyDead = new List<ICombatUnit>();
            foreach (var unit in heroParty.Units.Concat(monsterParty.Units))
            {
                if (unit.Character.HealthRatio <= 0 && !((FormationUnitInfo)unit.CombatInfo).IsDead)
                {
                    ((FormationUnitInfo)unit.CombatInfo).IsDead = true;
                    newlyDead.Add(unit);
                }
            }

            foreach (var dead in newlyDead)
            {
                if (dead.Character.IsMonster)
                    continue;
                var party = dead.Team == Team.Heroes ? heroParty.Units : monsterParty.Units;
                StressParty(party);
            }
        }

        /// <summary>Applies the party stress effect to surviving heroes.</summary>
        /// <param name="party">The party whose living heroes receive stress.</param>
        private void StressParty(List<ICombatUnit> party)
        {
            var effect = content.GetEffect(EffectIds.Stress2);
            if (effect == null || context == null)
                return;

            foreach (var unit in party)
            {
                if (unit.Character.IsMonster || ((FormationUnitInfo)unit.CombatInfo).IsDead)
                    continue;
                foreach (var subEffect in effect.SubEffects)
                    subEffect.ApplyInstant(null, unit, effect, context);
                context.ResolveOverstress(unit);
            }
        }
    }
}