using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Wpf.Data
{
    /// <summary>Provides the hero and animation sets available to the Spine demo screen.</summary>
    public interface ISpineDemoCatalog
    {
        /// <summary>Gets the hero names, alphabetically ordered.</summary>
        IReadOnlyList<string> GetCharacters();

        /// <summary>Gets the animation set names of a hero; the idle set comes first, the rest alphabetically.</summary>
        /// <param name="character">The hero name.</param>
        IReadOnlyList<string> GetAnimations(string character);

        /// <summary>Gets the absolute content directory (skeleton, atlas, page texture) of one animation set.</summary>
        /// <param name="character">The hero name.</param>
        /// <param name="animation">The animation set name.</param>
        string GetContentDirectory(string character, string animation);

        /// <summary>Finds the animation set of a hero whose skeleton declares one animation id.</summary>
        /// <param name="character">The hero name.</param>
        /// <param name="animationId">The skill art animation id ("smite", "zealous_accusation").</param>
        /// <returns>The animation set name, or null when the hero has no set for this id.</returns>
        string? FindAnimationSet(string character, string animationId);
    }
}
