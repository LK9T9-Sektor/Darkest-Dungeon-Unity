using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;

namespace Sektor.DarkestDungeon.Core.Combat.Character.Utils
{
    /// <summary>Helpers for combat attribute types.</summary>
    public static class CharacterHelper
    {
        /// <summary>Converts a string token to an <see cref="AttributeType"/>.</summary>
        /// <param name="typeString">The string token.</param>
        /// <returns>The corresponding attribute type, or <see cref="AttributeType.Undefined"/> if unknown.</returns>
        public static AttributeType StringToAttributeType(string typeString)
        {
            switch (typeString)
            {
                case "max_hp": return AttributeType.HitPoints;
                case "stress": return AttributeType.Stress;
                case "defense_rating": return AttributeType.DefenseRating;
                case "protection_rating": return AttributeType.ProtectionRating;
                case "speed_rating": return AttributeType.SpeedRating;
                case "attack_rating": return AttributeType.AttackRating;
                case "crit_chance": return AttributeType.CritChance;
                case "damage_low": return AttributeType.DamageLow;
                case "damage_high": return AttributeType.DamageHigh;
                case "armour": return AttributeType.ArmorDiscount;
                case "weapon": return AttributeType.WeaponDiscount;
                case "stun": return AttributeType.Stun;
                case "poison": return AttributeType.Poison;
                case "disease": return AttributeType.Disease;
                case "death_blow": return AttributeType.DeathBlow;
                case "move": return AttributeType.Move;
                case "bleed": return AttributeType.Bleed;
                case "debuff": return AttributeType.Debuff;
                case "trap": return AttributeType.Trap;
                case "hp_heal_amount": return AttributeType.HpHealAmount;
                case "hp_heal_percent": return AttributeType.HpHealPercent;
                case "stress_dmg_percent": return AttributeType.StressDmgPercent;
                case "stress_heal_percent": return AttributeType.StressHealPercent;
                case "damage_received_percent": return AttributeType.DmgReceivedPercent;
                case "hp_heal_received_percent": return AttributeType.HpHealReceivedPercent;
                case "stress_dmg_received_percent": return AttributeType.StressDmgReceivedPercent;
                case "stress_heal_received_percent": return AttributeType.StressHealReceivedPercent;
                case "stun_chance": return AttributeType.StunChance;
                case "poison_chance": return AttributeType.PoisonChance;
                case "bleed_chance": return AttributeType.BleedChance;
                case "move_chance": return AttributeType.MoveChance;
                case "debuff_chance": return AttributeType.DebuffChance;
                case "scouting_chance": return AttributeType.ScoutingChance;
                case "party_surprise_chance": return AttributeType.PartySurpriseChance;
                case "monsters_surprise_chance": return AttributeType.MonsterSurpirseChance;
                case "remove_negative_quirk_chance": return AttributeType.RemoveQuirkChance;
                case "food_consumption_percent": return AttributeType.FoodConsumption;
                case "starving_damage_percent": return AttributeType.StarvingDamagePercent;
                case "resolve_check_percent": return AttributeType.ResolveCheckPercent;
                case "resolve_xp_bonus_percent": return AttributeType.ResolveXpPercent;
                default:
                    return AttributeType.Undefined;
            }
        }

        /// <summary>Converts a buff stat type token to a <see cref="BuffType"/>.</summary>
        /// <param name="buffType">The token ("combat_stat_add"/"combat_stat_multiply").</param>
        /// <returns>The buff type, or <see cref="BuffType.StatAdd"/> if unknown.</returns>
        public static BuffType StringToBuffType(string buffType)
        {
            switch (buffType)
            {
                case "combat_stat_add":
                    return BuffType.StatAdd;
                case "combat_stat_multiply":
                    return BuffType.StatMultiply;
                default:
                    return BuffType.StatAdd;
            }
        }

        /// <summary>Converts a buff rule token to a <see cref="BuffRule"/>.</summary>
        /// <param name="buffRule">The rule token.</param>
        /// <returns>The buff rule, or <see cref="BuffRule.Always"/> if unknown.</returns>
        public static BuffRule StringToBuffRule(string buffRule)
        {
            switch (buffRule)
            {
                case "always": return BuffRule.Always;
                case "monsterSize": return BuffRule.Size;
                case "lightbelow": return BuffRule.LightBelow;
                case "lightabove": return BuffRule.LightAbove;
                case "hpbelow": return BuffRule.HpBelow;
                case "hpabove": return BuffRule.HpAbove;
                case "afflicted": return BuffRule.Afflicted;
                case "virtued": return BuffRule.Virtued;
                case "meleeonly": return BuffRule.Melee;
                case "rangedonly": return BuffRule.Ranged;
                case "firstroundonly": return BuffRule.FirstRound;
                case "actorStatus": return BuffRule.Status;
                case "monsterType": return BuffRule.EnemyType;
                case "at_deaths_door": return BuffRule.DeathsDoor;
                case "in_rank": return BuffRule.InRank;
                case "in_camp": return BuffRule.InCamp;
                case "in_mode": return BuffRule.InMode;
                case "in_dungeon": return BuffRule.InDungeon;
                case "stress_above": return BuffRule.StressAbove;
                case "stress_below": return BuffRule.StressBelow;
                case "walking_backwards": return BuffRule.WalkBack;
                case "in_activity": return BuffRule.InActivity;
                case "in_corridor": return BuffRule.InCorridor;
                case "riposte": return BuffRule.Riposting;
                case "skill": return BuffRule.Skill;
                default:
                    return BuffRule.Always;
            }
        }
    }
}
