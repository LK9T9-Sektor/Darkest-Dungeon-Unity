using System.Collections.Generic;
using UnityEngine;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Duel;

/// <summary>
/// Bridges the core battle feedback (<see cref="DuelBattleEvents.PopupShown"/>) into the Unity popup
/// layer: maps each <see cref="PopupType"/> to a short label and colour and places it over the unit.
/// </summary>
public class BattleEventsAdapter
{
    private readonly BattlePopupLayer _popups;

    private static readonly Dictionary<PopupType, string> Labels = new Dictionary<PopupType, string>
    {
        { PopupType.Miss, "MISS" },
        { PopupType.Dodge, "DODGE" },
        { PopupType.ZeroDamage, "0" },
        { PopupType.CritDamage, "CRIT!" },
        { PopupType.Pass, string.Empty },
        { PopupType.Tagged, "MARK" },
        { PopupType.Cured, "CURED" },
        { PopupType.Bleed, "BLEED" },
        { PopupType.Poison, "BLIGHT" },
        { PopupType.Stunned, "STUN" },
        { PopupType.BleedResist, "BLEED RESIST" },
        { PopupType.PoisonResist, "BLIGHT RESIST" },
        { PopupType.StunResist, "STUN RESIST" },
        { PopupType.MoveResist, "MOVE RESIST" },
        { PopupType.DebuffResist, "DEBUFF RESIST" },
        { PopupType.Buff, "BUFF" },
        { PopupType.Debuff, "DEBUFF" },
        { PopupType.Unstun, "UNSTUN" },
        { PopupType.Untagged, "UNTAGGED" },
        { PopupType.DiseaseResist, "DISEASE RESIST" },
        { PopupType.StressHeal, "STRESS HEAL" },
        { PopupType.Disease, "DISEASE" },
        { PopupType.PositiveQuirk, "QUIRK" },
        { PopupType.NegativeQuirk, "NEGATIVE QUIRK" },
        { PopupType.QuirkRemoved, "QUIRK REMOVED" },
        { PopupType.DeathsDoor, "DEATH'S DOOR" },
        { PopupType.DeathBlow, "DEATH BLOW" },
        { PopupType.HeartAttack, "HEART ATTACK" },
        { PopupType.Guard, "GUARD" },
        { PopupType.Riposte, "RIPOSTE" },
        { PopupType.DiseaseCured, "DISEASE CURED" },
        { PopupType.RetreatFailed, "RETREAT FAILED" }
    };

    private static readonly Color DamageColor = new Color(1f, 0.35f, 0.3f);
    private static readonly Color CritColor = new Color(1f, 0.65f, 0.2f);
    private static readonly Color HealColor = new Color(0.4f, 1f, 0.4f);
    private static readonly Color StressColor = new Color(0.75f, 0.5f, 1f);
    private static readonly Color NeutralColor = new Color(0.9f, 0.9f, 0.9f);
    private static readonly Color BuffColor = new Color(0.5f, 0.9f, 1f);
    private static readonly Color DebuffColor = new Color(1f, 0.5f, 1f);
    private static readonly Color DoomColor = new Color(0.7f, 0.1f, 0.1f);

    /// <summary>Initializes a new instance of the <see cref="BattleEventsAdapter"/> class.</summary>
    /// <param name="popups">The popup layer to render into.</param>
    public BattleEventsAdapter(BattlePopupLayer popups)
    {
        _popups = popups;
    }

    /// <summary>Subscribes to the duel's popup feedback.</summary>
    /// <param name="duel">The duel controller.</param>
    public void Attach(DuelController duel)
    {
        duel.Events.PopupShown += OnPopupShown;
    }

    /// <summary>Unsubscribes from the duel's popup feedback.</summary>
    /// <param name="duel">The duel controller.</param>
    public void Detach(DuelController duel)
    {
        duel.Events.PopupShown -= OnPopupShown;
    }

    private void OnPopupShown(ICombatUnit unit, PopupType type, string value)
    {
        if (_popups == null || unit == null || unit.CombatInfo == null)
            return;

        string label = FormatLabel(type, value);
        if (string.IsNullOrEmpty(label))
            return;

        _popups.ShowAt(unit.CombatInfo.CombatId, label, ColorFor(type));
    }

    private static string FormatLabel(PopupType type, string value)
    {
        switch (type)
        {
            case PopupType.Damage:
            case PopupType.CritDamage:
            case PopupType.Heal:
            case PopupType.CritHeal:
            case PopupType.Stress:
                return value;
            default:
                string label;
                return Labels.TryGetValue(type, out label) ? label : string.Empty;
        }
    }

    private static Color ColorFor(PopupType type)
    {
        switch (type)
        {
            case PopupType.Damage:
                return DamageColor;
            case PopupType.CritDamage:
                return CritColor;
            case PopupType.Heal:
            case PopupType.CritHeal:
            case PopupType.StressHeal:
            case PopupType.Cured:
                return HealColor;
            case PopupType.Stress:
                return StressColor;
            case PopupType.Buff:
            case PopupType.Guard:
                return BuffColor;
            case PopupType.Debuff:
                return DebuffColor;
            case PopupType.DeathsDoor:
            case PopupType.DeathBlow:
            case PopupType.HeartAttack:
                return DoomColor;
            default:
                return NeutralColor;
        }
    }
}