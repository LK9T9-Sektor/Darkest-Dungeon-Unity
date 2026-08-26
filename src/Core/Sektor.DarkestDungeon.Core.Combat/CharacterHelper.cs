using Sektor.DarkestDungeon.Core.Combat.Enums;

namespace Sektor.DarkestDungeon.Core.Combat
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
    }
}
