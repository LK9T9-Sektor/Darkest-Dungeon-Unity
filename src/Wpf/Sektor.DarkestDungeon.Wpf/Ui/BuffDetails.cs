using System;
using System.Collections.Generic;
using System.Globalization;

using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;

namespace Sektor.DarkestDungeon.Wpf.Ui
{
    /// <summary>Builds the human-readable name, description and duration of an applied buff.</summary>
    public static class BuffDetails
    {
        /// <summary>Gets the readable attribute labels used when a buff has no content id.</summary>
        private static readonly IReadOnlyDictionary<AttributeType, string> AttributeLabels =
            new Dictionary<AttributeType, string>
            {
                { AttributeType.HitPoints, "Max HP" },
                { AttributeType.Stress, "Stress" },
                { AttributeType.HpHealAmount, "HP Heal" },
                { AttributeType.HpHealPercent, "HP Heal" },
                { AttributeType.DmgReceivedPercent, "Damage Received" },
                { AttributeType.HpHealReceivedPercent, "HP Heal Received" },
                { AttributeType.StressDmgReceivedPercent, "Stress Received" },
                { AttributeType.StressDmgPercent, "Stress" },
                { AttributeType.StressHealPercent, "Stress Heal" },
                { AttributeType.StressHealReceivedPercent, "Stress Heal Received" },
                { AttributeType.ResolveCheckPercent, "Resolve Check" },
                { AttributeType.ResolveXpPercent, "Resolve XP" },
                { AttributeType.StunChance, "Stun" },
                { AttributeType.PoisonChance, "Blight" },
                { AttributeType.BleedChance, "Bleed" },
                { AttributeType.MoveChance, "Move" },
                { AttributeType.DebuffChance, "Debuff" },
                { AttributeType.ScoutingChance, "Scouting" },
                { AttributeType.PartySurpriseChance, "Party Surprise" },
                { AttributeType.MonsterSurpirseChance, "Monster Surprise" },
                { AttributeType.RemoveQuirkChance, "Quirk Removal" },
                { AttributeType.FoodConsumption, "Food Consumption" },
                { AttributeType.StarvingDamagePercent, "Starving Damage" },
                { AttributeType.DefenseRating, "Dodge" },
                { AttributeType.ProtectionRating, "Protection" },
                { AttributeType.SpeedRating, "Speed" },
                { AttributeType.AttackRating, "Accuracy" },
                { AttributeType.CritChance, "Crit" },
                { AttributeType.DamageLow, "Min Damage" },
                { AttributeType.DamageHigh, "Max Damage" },
                { AttributeType.ArmorDiscount, "Armor Discount" },
                { AttributeType.WeaponDiscount, "Weapon Discount" },
                { AttributeType.Stun, "Stun Resist" },
                { AttributeType.Poison, "Blight Resist" },
                { AttributeType.Disease, "Disease Resist" },
                { AttributeType.DeathBlow, "Death Blow Resist" },
                { AttributeType.Move, "Move Resist" },
                { AttributeType.Bleed, "Bleed Resist" },
                { AttributeType.Debuff, "Debuff Resist" },
                { AttributeType.Trap, "Trap Resist" },
            };

        /// <summary>Gets the attribute types whose modifier is a 0-1 fraction shown as a percentage.</summary>
        private static readonly HashSet<AttributeType> PercentAttributes = new HashSet<AttributeType>
        {
            AttributeType.HpHealPercent,
            AttributeType.DmgReceivedPercent,
            AttributeType.HpHealReceivedPercent,
            AttributeType.StressDmgReceivedPercent,
            AttributeType.StressDmgPercent,
            AttributeType.StressHealPercent,
            AttributeType.StressHealReceivedPercent,
            AttributeType.ResolveCheckPercent,
            AttributeType.ResolveXpPercent,
            AttributeType.StunChance,
            AttributeType.PoisonChance,
            AttributeType.BleedChance,
            AttributeType.MoveChance,
            AttributeType.DebuffChance,
            AttributeType.ScoutingChance,
            AttributeType.PartySurpriseChance,
            AttributeType.MonsterSurpirseChance,
            AttributeType.RemoveQuirkChance,
            AttributeType.FoodConsumption,
            AttributeType.StarvingDamagePercent,
            AttributeType.DefenseRating,
            AttributeType.ProtectionRating,
            AttributeType.SpeedRating,
            AttributeType.AttackRating,
            AttributeType.CritChance,
            AttributeType.Stun,
            AttributeType.Poison,
            AttributeType.Disease,
            AttributeType.DeathBlow,
            AttributeType.Move,
            AttributeType.Bleed,
            AttributeType.Debuff,
            AttributeType.Trap,
        };

        /// <summary>Builds the display name of a buff (its content id title, or the attribute label).</summary>
        /// <param name="buff">The buff definition, or null.</param>
        /// <returns>The display name.</returns>
        public static string FormatName(Buff? buff)
        {
            if (buff == null)
                return string.Empty;

            if (!string.IsNullOrEmpty(buff.Id))
                return DisplayNames.Title(buff.Id);

            return AttributeLabel(buff.AttributeType);
        }

        /// <summary>Builds the effect description of a buff (e.g. "+6% Accuracy" or "x1.12 Max Damage").</summary>
        /// <param name="buff">The buff definition, or null.</param>
        /// <returns>The effect description.</returns>
        public static string FormatDescription(Buff? buff)
        {
            if (buff == null)
                return string.Empty;

            string value = buff.Type == BuffType.StatMultiply
                ? "x" + buff.ModifierValue.ToString("0.##", CultureInfo.InvariantCulture)
                : FormatAdditive(buff.AttributeType, buff.ModifierValue);

            return value + " " + AttributeLabel(buff.AttributeType);
        }

        /// <summary>Builds the duration or charge text of an applied buff.</summary>
        /// <param name="info">The applied buff, or null.</param>
        /// <returns>The duration text.</returns>
        public static string FormatDuration(BuffInfo? info)
        {
            if (info == null)
                return string.Empty;

            switch (info.DurationType)
            {
                case BuffDurationType.Round:
                    return info.Duration > 0
                        ? info.Duration + " round" + (info.Duration == 1 ? string.Empty : "s")
                        : "Rounds";
                case BuffDurationType.Combat:
                    return "Combat";
                case BuffDurationType.Permanent:
                    return "Permanent";
                case BuffDurationType.Raid:
                    return "Raid";
                case BuffDurationType.Camp:
                    return "Camp";
                case BuffDurationType.Activity:
                    return "Activity";
                case BuffDurationType.QuestComplete:
                    return "Quest";
                case BuffDurationType.IdleTownVisit:
                    return "Town Visit";
                default:
                    return string.Empty;
            }
        }

        private static string AttributeLabel(AttributeType attributeType)
        {
            return AttributeLabels.TryGetValue(attributeType, out string label) ? label : attributeType.ToString();
        }

        private static string FormatAdditive(AttributeType attributeType, float modifierValue)
        {
            string sign = modifierValue >= 0 ? "+" : string.Empty;
            if (PercentAttributes.Contains(attributeType))
                return sign + Math.Round(modifierValue * 100) + "%";

            double rounded = Math.Round(modifierValue);
            return sign + (rounded == modifierValue ? ((int)rounded).ToString() : modifierValue.ToString("0.##"));
        }
    }
}