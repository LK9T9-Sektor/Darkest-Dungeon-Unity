using System.Collections.Generic;
using System.Linq;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Party;

namespace Sektor.DarkestDungeon.Core.Duel
{
    /// <summary>Duel battle context wiring a core <see cref="BattleGround"/> to a core <see cref="BattleSolver"/>.</summary>
    public class DuelBattleContext : IBattleContext
    {
        private readonly BattleSolver solver;
        private readonly IDuelContent content;

        /// <inheritdoc/>
        public IBattleGround BattleGround { get; }

        /// <inheritdoc/>
        public IBattleEvents Events { get; }

        /// <summary>Gets or sets the current torch amount.</summary>
        public int TorchAmount { get; set; } = 75;

        /// <summary>Initializes a new instance of the <see cref="DuelBattleContext"/> class.</summary>
        /// <param name="battleGround">The battlefield.</param>
        /// <param name="events">The event sink.</param>
        /// <param name="content">The content source (effects catalog).</param>
        public DuelBattleContext(IBattleGround battleGround, IBattleEvents events, IDuelContent content)
        {
            BattleGround = battleGround;
            Events = events;
            this.content = content;
            solver = new BattleSolver(this);
        }

        /// <inheritdoc/>
        public int MonsterNumber { get { return BattleGround.MonsterNumber; } }

        /// <inheritdoc/>
        public int HeroNumber { get { return BattleGround.HeroNumber; } }

        /// <inheritdoc/>
        public int MarkedHeroes { get { return BattleGround.MarkedHeroes; } }

        /// <inheritdoc/>
        public int AfflictedHeroes
        {
            get { return BattleGround.HeroParty.Units.FindAll(unit => unit.Character.IsAfflicted).Count; }
        }

        /// <inheritdoc/>
        public int VirtuedHeroes { get { return BattleGround.VirtuedHeroes; } }

        /// <inheritdoc/>
        public int DeathsDoorHeroes
        {
            get { return BattleGround.HeroParty.Units.FindAll(unit => unit.Character.AtDeathsDoor).Count; }
        }

        /// <inheritdoc/>
        public int TorchMeter { get { return TorchAmount; } }

        /// <inheritdoc/>
        public int RoundNumber { get { return BattleGround.Round.RoundNumber; } }

        /// <inheritdoc/>
        public int CampingTimeLeft { get { return 0; } }

        /// <inheritdoc/>
        public IReadOnlyList<ICombatUnit> AliveHeroes
        {
            get { return BattleGround.HeroParty.Units.FindAll(unit => !((FormationUnitInfo)unit.CombatInfo).IsDead); }
        }

        /// <inheritdoc/>
        public IReadOnlyList<ICombatUnit> AliveMonsters
        {
            get { return BattleGround.MonsterParty.Units.FindAll(unit => !((FormationUnitInfo)unit.CombatInfo).IsDead && !unit.IsCorpse); }
        }

        /// <inheritdoc/>
        public IReadOnlyList<ICombatUnit> AllHeroes { get { return BattleGround.HeroParty.Units; } }

        /// <inheritdoc/>
        public IReadOnlyList<ICombatUnit> AllMonsters { get { return BattleGround.MonsterParty.Units; } }

        /// <inheritdoc/>
        public List<ICombatUnit> GetSkillAvailableTargets(ICombatUnit performer, CombatSkill skill)
        {
            return solver.GetSkillAvailableTargets(performer, skill);
        }

        /// <inheritdoc/>
        public bool IsSkillUsable(ICombatUnit performer, CombatSkill skill)
        {
            return solver.IsSkillUsable(performer, skill);
        }

        /// <inheritdoc/>
        public void ApplyCombatUnitRules(ICombatUnit unit, ICombatUnit other, CombatSkill skill, bool isRiposte)
        {
            if (unit.Character is Character character)
                character.ApplyAllBuffRules(new BattleRulesContext(
                    unit, other, BattleGround, skill, TorchAmount, false, isRiposte, null, false));
        }

        /// <inheritdoc/>
        public void ApplyIdleUnitRules(ICombatUnit unit)
        {
            if (unit.Character is Character character)
                character.ApplyAllBuffRules(new BattleRulesContext(
                    unit, null, BattleGround, null, TorchAmount, false, false, null, false));
        }

        /// <inheritdoc/>
        public void ApplyEffectById(string effectId, ICombatUnit target, bool independent)
        {
            var effect = content.GetEffect(effectId);
            if (effect == null || target == null)
                return;

            // The duel has no queued-effect processor yet, so apply instantly.
            foreach (var subEffect in effect.SubEffects)
                subEffect.ApplyInstant(null, target, effect, this);
        }
    }
}