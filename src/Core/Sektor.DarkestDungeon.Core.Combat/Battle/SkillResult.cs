using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Battle
{
    /// <summary>Container for skill execution results.</summary>
    public class SkillResult
    {
        /// <summary>Gets or sets the skill that was used.</summary>
        public CombatSkill Skill { get; set; }

        /// <summary>Gets or sets the skill art info.</summary>
        public SkillArtInfo ArtInfo { get; set; }

        /// <summary>Gets or sets the current result entry.</summary>
        public SkillResultEntry Current { get; set; }

        /// <summary>Gets or sets a value indicating whether any target was hit.</summary>
        public bool HasHit { get; set; }

        /// <summary>Gets or sets a value indicating whether any target was zeroed.</summary>
        public bool HasZeroHealth { get; set; }

        /// <summary>Gets the list of applied effects.</summary>
        public List<Effect> AppliedEffects { get; private set; }

        /// <summary>Gets the list of skill result entries.</summary>
        public List<SkillResultEntry> SkillEntries { get; private set; }

        /// <summary>Gets a value indicating whether any entry has a crit effect.</summary>
        public bool HasCritEffect
        {
            get
            {
                for (int i = 0; i < SkillEntries.Count; i++)
                    if (SkillEntries[i].Type == Enums.SkillResultType.Crit && SkillEntries[i].CanCritReleaf)
                        return true;
                return false;
            }
        }

        /// <summary>Gets a value indicating whether any entry has a kill effect.</summary>
        public bool HasDeadEffect
        {
            get
            {
                for (int i = 0; i < SkillEntries.Count; i++)
                    if (SkillEntries[i].IsZeroed && SkillEntries[i].CanKillReleaf)
                        return true;
                return false;
            }
        }

        /// <summary>Initializes a new instance of the <see cref="SkillResult"/> class.</summary>
        public SkillResult()
        {
            AppliedEffects = new List<Effect>();
            SkillEntries = new List<SkillResultEntry>();
        }

        /// <summary>Resets the result for reuse.</summary>
        public void Reset()
        {
            Current = null;
            HasHit = false;
            HasZeroHealth = false;
            AppliedEffects.Clear();
            SkillEntries.Clear();
        }

        /// <summary>Creates a copy of this result.</summary>
        /// <returns>A new <see cref="SkillResult"/> with the same values.</returns>
        public SkillResult Copy()
        {
            var copy = new SkillResult();
            copy.Skill = Skill;
            copy.ArtInfo = ArtInfo;
            copy.Current = Current;
            copy.HasHit = HasHit;
            copy.HasZeroHealth = HasZeroHealth;
            copy.AppliedEffects = new List<Effect>(AppliedEffects);
            copy.SkillEntries = new List<SkillResultEntry>(SkillEntries);
            return copy;
        }

        /// <summary>Adds a result entry.</summary>
        /// <param name="entry">The entry to add.</param>
        public void AddResultEntry(SkillResultEntry entry)
        {
            Current = entry;
            SkillEntries.Add(entry);
            if (entry.Type != Enums.SkillResultType.Dodge && entry.Type != Enums.SkillResultType.Miss)
                HasHit = true;
            if (entry.IsZeroed)
                HasZeroHealth = true;
        }

        /// <summary>Adds an applied effect.</summary>
        /// <param name="entry">The effect to add.</param>
        public void AddEffectEntry(Effect entry)
        {
            AppliedEffects.Add(entry);
        }
    }
}
