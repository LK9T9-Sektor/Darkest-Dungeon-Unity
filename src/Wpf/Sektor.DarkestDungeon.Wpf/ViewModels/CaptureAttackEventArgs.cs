using System;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Reports the attack the capture replay is currently playing.</summary>
    public sealed class CaptureAttackEventArgs : EventArgs
    {
        /// <summary>Initializes a new instance of the <see cref="CaptureAttackEventArgs"/> class.</summary>
        /// <param name="attacker">The attacking unit, if the capture knows it.</param>
        /// <param name="target">The attacked unit, if the capture knows it.</param>
        /// <param name="skillName">The skill id recorded for the attack.</param>
        public CaptureAttackEventArgs(IBattleFieldUnit? attacker, IBattleFieldUnit? target, string? skillName = null)
        {
            Attacker = attacker;
            Target = target;
            SkillName = skillName;
        }

        /// <summary>Gets the attacking unit.</summary>
        public IBattleFieldUnit? Attacker { get; }

        /// <summary>Gets the attacked unit.</summary>
        public IBattleFieldUnit? Target { get; }

        /// <summary>Gets the skill id recorded for the attack.</summary>
        public string? SkillName { get; }
    }
}
