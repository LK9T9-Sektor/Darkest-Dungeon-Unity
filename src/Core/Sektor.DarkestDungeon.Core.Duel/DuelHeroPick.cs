using System;
using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Duel
{
    /// <summary>A hero pick: class id + deterministic seed + optional active skills and quirks.</summary>
    public class DuelHeroPick
    {
        /// <summary>Gets the class id.</summary>
        public string ClassId { get; }

        /// <summary>Gets the deterministic seed.</summary>
        public int Seed { get; }

        /// <summary>Gets the active combat skill ids selected by the player (empty = all class skills).</summary>
        public IReadOnlyList<string> SelectedSkillIds { get; }

        /// <summary>Gets the quirk ids chosen for the hero.</summary>
        public IReadOnlyList<string> QuirkIds { get; }

        /// <summary>Gets the trinket ids equipped on the hero (up to two).</summary>
        public IReadOnlyList<string> TrinketIds { get; }

        /// <summary>Initializes a new instance of the <see cref="DuelHeroPick"/> class.</summary>
        /// <param name="classId">The class id.</param>
        /// <param name="seed">The seed.</param>
        /// <param name="selectedSkillIds">The selected active skill ids.</param>
        /// <param name="quirkIds">The quirk ids.</param>
        /// <param name="trinketIds">The equipped trinket ids.</param>
        public DuelHeroPick(string classId, int seed, IReadOnlyList<string> selectedSkillIds = null, IReadOnlyList<string> quirkIds = null, IReadOnlyList<string> trinketIds = null)
        {
            ClassId = classId;
            Seed = seed;
            SelectedSkillIds = selectedSkillIds ?? Array.Empty<string>();
            QuirkIds = quirkIds ?? Array.Empty<string>();
            TrinketIds = trinketIds ?? Array.Empty<string>();
        }
    }
}