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
    /// Detects newly dead units and applies the death's door mechanic to heroes. Monsters die at
    /// zero health (unless their death class forbids direct damage); heroes enter death's door on
    /// the first hit to zero and roll a death blow resistance check on every subsequent hit, dying
    /// when the roll fails. Hero death stresses the surviving party.
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
        /// <param name="content">The content source (for the stress effect and death's door buffs).</param>
        /// <param name="context">The duel battle context.</param>
        public DeathCheck(FormationParty heroParty, FormationParty monsterParty, IDuelContent content, DuelBattleContext context)
        {
            this.heroParty = heroParty;
            this.monsterParty = monsterParty;
            this.content = content;
            this.context = context;
        }

        /// <summary>Marks newly dead units and applies the death's door / death blow mechanics.</summary>
        public void Check()
        {
            var newlyDead = new List<ICombatUnit>();
            var newlyAtDeathsDoor = new List<ICombatUnit>();

            foreach (var unit in heroParty.Units.Concat(monsterParty.Units))
            {
                if (unit.Character.HealthRatio > 0 || ((FormationUnitInfo)unit.CombatInfo).IsDead)
                    continue;

                if (!unit.Character.SupportsDeathDoor)
                {
                    if (unit.Character is Monster monster && !monster.CanDieFromDamage)
                        continue;

                    ((FormationUnitInfo)unit.CombatInfo).IsDead = true;
                    newlyDead.Add(unit);
                    continue;
                }

                var hero = unit.Character as Hero;
                if (hero == null || hero.AtDeathsDoor || unit.CombatInfo.MarkedForDeath)
                    continue;

                EnterDeathsDoor(unit, hero);
                newlyAtDeathsDoor.Add(unit);
            }

            foreach (var unit in heroParty.Units.Concat(monsterParty.Units))
            {
                if (unit.Character.HealthRatio > 0 || ((FormationUnitInfo)unit.CombatInfo).IsDead)
                    continue;

                var hero = unit.Character as Hero;
                if (hero == null || !hero.SupportsDeathDoor)
                    continue;
                if (!hero.AtDeathsDoor && !unit.CombatInfo.MarkedForDeath)
                    continue;

                if (RollSurvival(unit, hero))
                    continue;

                ((FormationUnitInfo)unit.CombatInfo).IsDead = true;
                newlyDead.Add(unit);
            }

            foreach (var dead in newlyDead)
            {
                if (dead.Character.IsMonster)
                    continue;
                var party = dead.Team == Team.Heroes ? heroParty.Units : monsterParty.Units;
                StressParty(party);
            }
        }

        private void EnterDeathsDoor(ICombatUnit unit, Hero hero)
        {
            hero.ApplyDeathDoor(ResolveBuffs(hero.HeroClass?.DeathDoor?.Buffs));

            var survivalBuff = BuffIds.DeathsDoorSurvivalDebuff();
            unit.Character.AddBuff(new BuffInfo(survivalBuff, BuffDurationType.Combat, BuffSourceType.Adventure,
                BattleConstants.DeathsDoorSurvivalDuration));

            ApplyEffectById(EffectIds.BarkStress, unit);
            context.Events.ShowPopup(unit, PopupType.DeathsDoor);
        }

        private bool RollSurvival(ICombatUnit unit, Hero hero)
        {
            if (unit.CombatInfo.MarkedForDeath)
                return false;

            float resistIgnoreBonus = unit.Team == Team.Heroes
                ? (heroParty.Units.Count > monsterParty.Units.Count ? BattleConstants.ResistOverrideBonus : 0f)
                : (heroParty.Units.Count < monsterParty.Units.Count ? BattleConstants.ResistOverrideBonus : 0f);

            if (!RandomSolver.CheckSuccess(hero.DeathResist - resistIgnoreBonus))
                return false;

            var survivalBuff = BuffIds.DeathsDoorSurvivalDebuff();
            unit.Character.AddBuff(new BuffInfo(survivalBuff, BuffDurationType.Combat, BuffSourceType.Adventure,
                BattleConstants.DeathsDoorSurvivalDuration));
            context.Events.ShowPopup(unit, PopupType.DeathBlow);
            return true;
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

        private void ApplyEffectById(string effectId, ICombatUnit unit)
        {
            var effect = content.GetEffect(effectId);
            if (effect == null)
                return;
            foreach (var subEffect in effect.SubEffects)
                subEffect.ApplyInstant(null, unit, effect, context);
        }

        private List<Buff> ResolveBuffs(IReadOnlyList<string> buffIds)
        {
            var buffs = new List<Buff>();
            if (buffIds == null)
                return buffs;

            foreach (var buffId in buffIds)
            {
                var buff = content.GetBuff(buffId);
                if (buff != null)
                    buffs.Add(buff);
            }
            return buffs;
        }
    }
}