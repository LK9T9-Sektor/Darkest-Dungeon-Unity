using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Content.Character;

namespace Sektor.DarkestDungeon.Core.Duel
{
    /// <summary>Content source required to build duel parties from picks (hero classes, quirks, buffs, effects).</summary>
    public interface IDuelContent
    {
        /// <summary>Gets a hero class by its id, or null when unknown.</summary>
        HeroClass GetHeroClass(string classId);

        /// <summary>Gets a quirk by its id, or null when unknown.</summary>
        Quirk GetQuirk(string quirkId);

        /// <summary>Gets a buff by its id, or null when unknown.</summary>
        Buff GetBuff(string buffId);

        /// <summary>Gets an effect definition by its name, or null when unknown.</summary>
        Effect GetEffect(string effectId);
    }
}