using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Duel.Fight
{
    /// <summary>A hero fighter defined by its class, generation seed, selected combat skills and quirks.</summary>
    public sealed class HeroFightUnitSpec : FightUnitSpec
    {
        /// <summary>Initializes a new instance of the <see cref="HeroFightUnitSpec"/> class.</summary>
        /// <param name="classId">The hero class id.</param>
        /// <param name="seed">The deterministic generation seed.</param>
        /// <param name="skillIds">The selected combat skill ids.</param>
        /// <param name="quirkIds">The quirk or disease ids.</param>
        public HeroFightUnitSpec(string classId, int seed, IReadOnlyList<string> skillIds, IReadOnlyList<string> quirkIds)
        {
            ClassId = classId;
            Seed = seed;
            SkillIds = skillIds;
            QuirkIds = quirkIds;
        }

        /// <summary>Gets the hero class id.</summary>
        public string ClassId { get; }

        /// <summary>Gets the deterministic generation seed.</summary>
        public int Seed { get; }

        /// <summary>Gets the selected combat skill ids.</summary>
        public IReadOnlyList<string> SkillIds { get; }

        /// <summary>Gets the quirk or disease ids.</summary>
        public IReadOnlyList<string> QuirkIds { get; }
    }
}