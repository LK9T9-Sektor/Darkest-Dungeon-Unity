using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Content.Character;
using Sektor.DarkestDungeon.Core.Duel;
using Sektor.DarkestDungeon.Wpf.Combat;

namespace Sektor.DarkestDungeon.Wpf.Data
{
    /// <summary>WPF content source for the core duel module, backed by the local content catalogs.</summary>
    public sealed class DuelContent : IDuelContent
    {
        /// <inheritdoc/>
        public HeroClass GetHeroClass(string classId)
        {
            return DuelClasses.Get(classId);
        }

        /// <inheritdoc/>
        public Quirk GetQuirk(string quirkId)
        {
            return QuirkCatalog.Get(quirkId);
        }

        /// <inheritdoc/>
        public Buff GetBuff(string buffId)
        {
            return BuffCatalog.Get(buffId);
        }
    }
}