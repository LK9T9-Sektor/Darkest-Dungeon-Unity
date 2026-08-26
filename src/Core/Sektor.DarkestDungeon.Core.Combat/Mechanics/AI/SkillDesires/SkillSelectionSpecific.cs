using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Character.Components;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Events;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.AI
{
    /// <summary>Skill selection desire that selects a specific skill with per-round chance escalation.</summary>
    public sealed class SkillSelectionSpecific : SkillSelectionDesire
    {
        private string CombatSkillId { get; set; }
        private bool FirstInitiativeOnly { get; set; }
        private int PerRoundChance { get; set; }

        /// <inheritdoc/>
        public override int Chance
        {
            get
            {
                if (BattleContext != null)
                    return base.Chance + PerRoundChance * (BattleContext.BattleGround.Round.RoundNumber - 1);
                return base.Chance;
            }
            set
            {
                base.Chance = value;
            }
        }

        private IBattleContext BattleContext { get; set; }

        /// <summary>Initializes a new instance of the <see cref="SkillSelectionSpecific"/> class.</summary>
        /// <param name="dataSet">The data set to initialize from.</param>
        public SkillSelectionSpecific(Dictionary<string, object> dataSet)
        {
            GenerateFromDataSet(dataSet);
        }

        /// <summary>Sets the battle context used to resolve round-based chance.</summary>
        /// <param name="battleContext">The battle context.</param>
        public void SetBattleContext(IBattleContext battleContext)
        {
            BattleContext = battleContext;
        }

        /// <inheritdoc/>
        protected override bool IsRestricted(ICombatUnit performer, IBattleContext battleContext)
        {
            if (base.IsRestricted(performer, battleContext))
                return true;

            if (FirstInitiativeOnly && performer.CombatInfo.CurrentInitiative != 1)
                return true;

            return false;
        }

        /// <inheritdoc/>
        protected override bool IsValidSkill(ICombatUnit performer, CombatSkill skill, IBattleContext battleContext)
        {
            if (!base.IsValidSkill(performer, skill, battleContext))
                return false;

            return skill.Id == CombatSkillId;
        }

        /// <inheritdoc/>
        protected override void GenerateFromDataSet(Dictionary<string, object> dataSet)
        {
            foreach (var token in dataSet)
            {
                switch (token.Key)
                {
                    case "hp_ratio_treshold":
                        break;
                    case "combat_skill_id":
                        CombatSkillId = (string)dataSet["combat_skill_id"];
                        break;
                    case "per_round_chance":
                        PerRoundChance = Chance = (int)((double)dataSet[token.Key] * 100);
                        break;
                    case "first_initiative_only":
                        FirstInitiativeOnly = (bool)dataSet[token.Key];
                        break;
                    default:
                        ProcessBaseDataToken(token);
                        break;
                }
            }
        }
    }
}
