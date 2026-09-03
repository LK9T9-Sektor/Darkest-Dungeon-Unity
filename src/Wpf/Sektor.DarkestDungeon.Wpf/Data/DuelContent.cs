using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sektor.DarkestDungeon.Clients.Content;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Content.Camping;
using Sektor.DarkestDungeon.Core.Content.Character;
using Sektor.DarkestDungeon.Core.Content.Trinket;
using Sektor.DarkestDungeon.Core.Duel;
using Sektor.DarkestDungeon.Wpf.Combat;

namespace Sektor.DarkestDungeon.Wpf.Data
{
    /// <summary>WPF content source for the core duel module, backed by the local content catalogs.</summary>
    public sealed class DuelContent : IDuelContent
    {
        private static readonly EffectCatalog Effects = DuelClasses.Effects;
        private static readonly List<Trait> Afflictions;
        private static readonly List<Trait> Virtues;
        private static readonly MonsterCatalog Monsters;
        private static readonly MonsterBrainCatalog Brains;

        static DuelContent()
        {
            var traits = GameDataReader.ReadTraits(LoadContentText("Traits", "JsonTraits.json"));
            Afflictions = traits.Where(trait => trait.IsAffliction).ToList();
            Virtues = traits.Where(trait => trait.IsVirtue).ToList();

            string monstersFolder = Path.Combine(AppContext.BaseDirectory, "Content", "Monsters");
            if (Directory.Exists(monstersFolder))
            {
                var fileContents = new List<string>();
                foreach (string path in Directory.EnumerateFiles(monstersFolder, "*.txt"))
                    fileContents.Add(File.ReadAllText(path));
                Monsters = MonsterCatalog.Load(fileContents, Effects);
            }
            else
            {
                Monsters = new MonsterCatalog(null);
            }

            string brainsText = LoadContentText("AI", "JsonAI.json");
            Brains = string.IsNullOrEmpty(brainsText) ? new MonsterBrainCatalog(null) : GameDataReader.ReadBrains(brainsText);
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
        public Sektor.DarkestDungeon.Core.Content.Trinket.Trinket GetTrinket(string trinketId)
        {
            return TrinketCatalog.Get(trinketId);
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

        /// <inheritdoc/>
        public MonsterClass GetMonsterClass(string monsterId)
        {
            MonsterClass monster;
            return Monsters.TryGet(monsterId, out monster) ? monster : null;
        }

        /// <inheritdoc/>
        public MonsterBrain GetMonsterBrain(string brainId)
        {
            MonsterBrain brain;
            return Brains.TryGet(brainId, out brain) ? brain : null;
        }

        private static string LoadContentText(string folder, string fileName)
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Content", folder, fileName);
            return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        }
    }
}