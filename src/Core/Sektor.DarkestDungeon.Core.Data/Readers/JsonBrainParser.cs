using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Data.Dto;

namespace Sektor.DarkestDungeon.Core.Data.Readers
{
    /// <summary>
    /// Parses the JsonAI.json content file into core <see cref="MonsterBrain"/> instances using the
    /// campaign wire-keys, mirroring the legacy Unity reader behavior.
    /// </summary>
    public sealed class JsonBrainParser
    {
        private const string RandomSkillKey = "random_skill";
        private const string PreferredSkillKey = "preferred_skill";
        private const string HealSkillKey = "heal_skill";
        private const string SpecificSkillKey = "specific_skill";
        private const string PerformingTurnSkillKey = "performing_turn_skill";
        private const string AllyAliveSkillKey = "ally_alive_skill";
        private const string FillAllyCaptorEmptySkillKey = "fill_ally_captor_empty_skill";
        private const string EffectKeyStatusSkillKey = "effect_key_status_skill";
        private const string AllyDeadSkillKey = "ally_dead_skill";
        private const string RandomTargetKey = "random_target";
        private const string MarkedTargetKey = "marked_target";
        private const string HealthTargetKey = "health_target";
        private const string StressTargetKey = "stress_target";
        private const string FillAllyCaptorEmptyTargetKey = "fill_ally_captor_empty_target";
        private const string RankTargetKey = "rank_target";
        private const string AllyClassTargetKey = "ally_class_target";
        private const string ResistanceTargetKey = "resistance_target";
        private const string HpRatioThresholdKey = "hp_ratio_threshold";
        private const string DeathKey = "death";
        private const string GuaranteedKey = "guaranteed";
        private const string AllyLastDamagedKey = "ally_last_damaged";
        private const string LastSkillKey = "last_skill";
        private const string AllyActorClassCountKey = "ally_actor_class_count";

        /// <summary>Gets the skill selection registry mapping wire-keys to desire factories.</summary>
        internal IReadOnlyDictionary<string, Func<Dictionary<string, object>, SkillSelectionDesire>> SkillDesires { get; }

        /// <summary>Gets the target selection registry mapping wire-keys to desire factories.</summary>
        internal IReadOnlyDictionary<string, Func<Dictionary<string, object>, TargetSelectionDesire>> TargetDesires { get; }

        /// <summary>Gets the bonus initiative registry mapping wire-keys to desire factories.</summary>
        internal IReadOnlyDictionary<string, Func<Dictionary<string, object>, BonusInitiativeDesire>> BonusDesires { get; }

        /// <summary>Initializes a new instance of the <see cref="JsonBrainParser"/> class.</summary>
        public JsonBrainParser()
        {
            SkillDesires = new Dictionary<string, Func<Dictionary<string, object>, SkillSelectionDesire>>
            {
                { RandomSkillKey, data => new SkillSelectionRandom(data) },
                { PreferredSkillKey, data => new SkillSelectionPreferred(data) },
                { HealSkillKey, data => new SkillSelectionHeal(data) },
                { SpecificSkillKey, data => new SkillSelectionSpecific(data) },
                { PerformingTurnSkillKey, data => new SkillSelectionPerformingTurn(data) },
                { AllyAliveSkillKey, data => new SkillSelectionAllyAlive(data) },
                { FillAllyCaptorEmptySkillKey, data => new SkillSelectionFillEmptyCaptor(data) },
                { EffectKeyStatusSkillKey, data => new SkillSelectionStatus(data) },
                { AllyDeadSkillKey, data => new SkillSelectionAllyDead(data) }
            };

            TargetDesires = new Dictionary<string, Func<Dictionary<string, object>, TargetSelectionDesire>>
            {
                { RandomTargetKey, data => new TargetSelectionRandom(data) },
                { MarkedTargetKey, data => new TargetSelectionMarked(data) },
                { HealthTargetKey, data => new TargetSelectionHealth(data) },
                { StressTargetKey, data => new TargetSelectionStress(data) },
                { FillAllyCaptorEmptyTargetKey, data => new TargetSelectionFillCaptor(data) },
                { RankTargetKey, data => new TargetSelectionRank(data) },
                { AllyClassTargetKey, data => new TargetSelectionAllyClass(data) },
                { ResistanceTargetKey, data => new TargetSelectionResistance(data) }
            };

            BonusDesires = new Dictionary<string, Func<Dictionary<string, object>, BonusInitiativeDesire>>
            {
                { HpRatioThresholdKey, data => new BonusInitiativeHpRatio(data) },
                { DeathKey, data => new BonusInitiativeDeath(data) },
                { GuaranteedKey, data => new BonusInitiativeGuaranteed(data) },
                { AllyLastDamagedKey, data => new BonusInitiativeAllyLastDamaged(data) },
                { LastSkillKey, data => new BonusInitiativeLastSkill(data) },
                { AllyActorClassCountKey, data => new BonusInitiativeAllyClassCount(data) }
            };
        }

        /// <summary>Parses the given JsonAI.json text into monster brains.</summary>
        /// <param name="jsonText">The JsonAI.json file content.</param>
        /// <returns>The parsed monster brains, one per entry.</returns>
        public List<MonsterBrain> Parse(string jsonText)
        {
            var brains = new List<MonsterBrain>();
            JsonMonsterBrainsDatabase root = JsonConvert.DeserializeObject<JsonMonsterBrainsDatabase>(jsonText);

            if (root == null || root.monster_brains == null)
                return brains;

            foreach (JsonMonsterBrains jsonBrain in root.monster_brains)
            {
                var brain = new MonsterBrain { Id = jsonBrain.id };

                AddCooldowns(brain, jsonBrain.skill_cooldowns);
                AddSkillDesires(brain, jsonBrain.skill_selection_desires);
                AddTargetDesires(brain, jsonBrain.target_selection_desires);
                AddBonusDesires(brain, jsonBrain.bonus_initiative_desires);

                brains.Add(brain);
            }

            return brains;
        }

        private void AddCooldowns(MonsterBrain brain, List<JsonSkillCooldown> cooldowns)
        {
            if (cooldowns == null)
                return;

            foreach (JsonSkillCooldown cooldown in cooldowns)
                brain.SkillCooldowns.Add(new SkillCooldown(cooldown.combat_skill_id, cooldown.amount));
        }

        private void AddSkillDesires(MonsterBrain brain, List<JsonSelectionDesire> desires)
        {
            if (desires == null)
                return;

            IReadOnlyDictionary<string, Func<Dictionary<string, object>, SkillSelectionDesire>> factories = SkillDesires;
            foreach (JsonSelectionDesire desire in desires)
            {
                factories.TryGetValue(desire.type, out var factory);
                if (factory != null)
                    brain.SkillDesireSet.Add(factory(SafeDataSet(desire.data)));
            }
        }

        private void AddTargetDesires(MonsterBrain brain, List<JsonSelectionDesire> desires)
        {
            if (desires == null)
                return;

            IReadOnlyDictionary<string, Func<Dictionary<string, object>, TargetSelectionDesire>> factories = TargetDesires;
            foreach (JsonSelectionDesire desire in desires)
            {
                factories.TryGetValue(desire.type, out var factory);
                if (factory != null)
                    brain.TargetDesireSet.Add(factory(SafeDataSet(desire.data)));
            }
        }

        private void AddBonusDesires(MonsterBrain brain, List<JsonSelectionDesire> desires)
        {
            if (desires == null)
                return;

            IReadOnlyDictionary<string, Func<Dictionary<string, object>, BonusInitiativeDesire>> factories = BonusDesires;
            foreach (JsonSelectionDesire desire in desires)
            {
                factories.TryGetValue(desire.type, out var factory);
                if (factory != null)
                    brain.BonusDesireSet.Add(factory(SafeDataSet(desire.data)));
            }
        }

        private static Dictionary<string, object> SafeDataSet(Dictionary<string, object> data)
        {
            return data ?? new Dictionary<string, object>();
        }
    }
}