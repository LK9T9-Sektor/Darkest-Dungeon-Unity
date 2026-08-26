using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Combat.Skills
{
    /// <summary>Rank-based targeting set parsed from formation strings.</summary>
    public class FormationSet
    {
        /// <summary>Gets a value indicating whether this is a multitarget formation.</summary>
        public bool IsMultitarget { get; private set; }

        /// <summary>Gets a value indicating whether this is a random target formation.</summary>
        public bool IsRandomTarget { get; private set; }

        /// <summary>Gets a value indicating whether this targets the performer's own formation.</summary>
        public bool IsSelfFormation { get; private set; }

        /// <summary>Gets a value indicating whether this targets the performer itself.</summary>
        public bool IsSelfTarget { get; private set; }

        /// <summary>Gets the list of valid ranks.</summary>
        public List<int> Ranks { get; private set; }

        /// <summary>Gets the skill target type based on formation flags.</summary>
        public Enums.SkillTargetType SkillTargetType
        {
            get
            {
                if (IsSelfTarget)
                    return Enums.SkillTargetType.Self;

                return IsSelfFormation ? Enums.SkillTargetType.Party : Enums.SkillTargetType.Enemy;
            }
        }

        /// <summary>Initializes a new instance of the <see cref="FormationSet"/> class.</summary>
        /// <param name="formationString">The formation string to parse.</param>
        public FormationSet(string formationString)
        {
            FromString(formationString);
        }

        /// <summary>Checks if the skill can be launched from the given rank.</summary>
        /// <param name="rank">The performer's rank.</param>
        /// <param name="size">The performer's size.</param>
        /// <returns>True if launchable from the given rank.</returns>
        public bool IsLaunchableFrom(int rank, int size)
        {
            return Ranks.Exists(r => r >= rank && r <= rank + size - 1);
        }

        /// <summary>Checks if the given unit is a valid target.</summary>
        /// <param name="rank">The target's rank.</param>
        /// <param name="size">The target's size.</param>
        /// <returns>True if the unit is a valid target.</returns>
        public bool IsTargetableUnit(int rank, int size)
        {
            return Ranks.Exists(r => r >= rank && r <= rank + size - 1);
        }

        private void FromString(string formationString)
        {
            IsMultitarget = false;
            IsRandomTarget = false;
            IsSelfFormation = false;

            Ranks = new List<int>();

            if (formationString == "")
            {
                IsSelfTarget = true;
                IsSelfFormation = true;
                return;
            }

            while (formationString[0] == '@' || formationString[0] == '~' || formationString[0] == '?')
            {
                if (formationString[0] == '@')
                    IsSelfFormation = true;
                if (formationString[0] == '~')
                    IsMultitarget = true;
                if (formationString[0] == '?')
                    IsRandomTarget = true;

                formationString = formationString.Substring(1);
            }

            for (int i = 0; i < formationString.Length; i++)
                Ranks.Add(int.Parse(formationString[i].ToString()));

            Ranks.Sort();
        }
    }
}
