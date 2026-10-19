using System.Collections.Generic;
using Sektor.DarkestDungeon.Wpf.Ui;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>
    /// The battle-screen surface the <see cref="Views.DuelBattleView"/> code-behind reads: the acting
    /// side, the selected/rival skill badge and the arrow targets. Implemented by the duel view model
    /// and the PvE view model so the same view renders both modes.
    /// </summary>
    public interface IDuelBattleViewData
    {
        /// <summary>Gets a value indicating whether it is the local player's turn to act.</summary>
        bool IsLocalTurn { get; }

        /// <summary>Gets a value indicating whether the move mode (adjacent rank swap) is active.</summary>
        bool IsMoveMode { get; }

        /// <summary>Gets a value indicating whether the rival preview is a move rather than a skill arrow.</summary>
        bool IsMovePreview { get; }

        /// <summary>Gets a value indicating whether the selected skill targets multiple units at once.</summary>
        bool SelectedSkillIsMultiTarget { get; }

        /// <summary>Gets the currently selected skill (badge source), or null.</summary>
        DuelSkillViewModel? SelectedSkill { get; }

        /// <summary>Gets the rival (AI) skill preview shown in the badge, or null.</summary>
        DuelSkillViewModel? AiSkillPreview { get; }

        /// <summary>Gets the rival (AI) target preview card, or null.</summary>
        DuelUnitViewModel? AiTargetPreview { get; }

        /// <summary>Gets the tone of the selected skill for the arrow color.</summary>
        SkillTone SelectedSkillTone { get; }

        /// <summary>Gets the hero side unit cards.</summary>
        IEnumerable<DuelUnitViewModel> Heroes { get; }

        /// <summary>Gets the monster side unit cards.</summary>
        IEnumerable<DuelUnitViewModel> Monsters { get; }

        /// <summary>Whether the hovered unit can show the attack arrow.</summary>
        /// <param name="target">The hovered unit card.</param>
        bool CanShowArrow(DuelUnitViewModel target);
    }
}