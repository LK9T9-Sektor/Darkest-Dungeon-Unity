using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Content.Character;
using Sektor.DarkestDungeon.Core.Duel;
using Sektor.DarkestDungeon.Wpf.Combat;

namespace Sektor.DarkestDungeon.Wpf.Data
{
    /// <summary>WPF content source for the core duel module, backed by the local content catalogs.</summary>
    public sealed class DuelContent : IDuelContent
    {
        private static readonly EffectCatalog Effects = EffectCatalog.Load(LoadEffectsText());
        private static readonly List<Trait> Afflictions;
        private static readonly List<Trait> Virtues;

        static DuelContent()
        {
            var data = JsonConvert.DeserializeObject<JsonTraitData>(LoadTraitsText());
            var traits = TraitMapper.Parse(data?.traits);
            Afflictions = traits.Where(trait => trait.IsAffliction).ToList();
            Virtues = traits.Where(trait => trait.IsVirtue).ToList();
        }

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

        /// <inheritdoc/>
        public Effect GetEffect(string effectId)
        {
            return Effects.Get(effectId);
        }

        /// <inheritdoc/>
        public IReadOnlyList<Trait> GetAfflictions()
        {
            return Afflictions;
        }

        /// <inheritdoc/>
        public IReadOnlyList<Trait> GetVirtues()
        {
            return Virtues;
        }

        private static string LoadEffectsText()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Content", "Effects", "Effects.txt");
            return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        }

        private static string LoadTraitsText()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Content", "Traits", "JsonTraits.json");
            return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        }
    }
}