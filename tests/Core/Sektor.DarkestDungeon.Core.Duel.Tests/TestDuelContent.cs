using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Character.Utils;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Content.Character;
using Sektor.DarkestDungeon.Core.Content.Database;

namespace Sektor.DarkestDungeon.Core.Duel.Tests
{
    /// <summary>Content source for duel tests, loaded from the bundled content files (heroes, quirks, buffs, effects).</summary>
    public sealed class TestDuelContent : IDuelContent
    {
        private readonly Dictionary<string, HeroClass> classes;
        private readonly Dictionary<string, Quirk> quirks;
        private readonly Dictionary<string, Buff> buffs;
        private readonly EffectCatalog effects;

        /// <summary>Initializes a new instance of the <see cref="TestDuelContent"/> class from the bundled content.</summary>
        public TestDuelContent()
        {
            classes = LoadHeroClasses();
            quirks = LoadQuirks();
            buffs = LoadBuffs();
            effects = LoadEffects();
        }

        /// <inheritdoc/>
        public HeroClass GetHeroClass(string classId)
        {
            HeroClass heroClass;
            return classes.TryGetValue(classId, out heroClass) ? heroClass : null;
        }

        /// <inheritdoc/>
        public Quirk GetQuirk(string quirkId)
        {
            Quirk quirk;
            return quirks.TryGetValue(quirkId, out quirk) ? quirk : null;
        }

        /// <inheritdoc/>
        public Buff GetBuff(string buffId)
        {
            Buff buff;
            return buffs.TryGetValue(buffId, out buff) ? buff : null;
        }

        /// <inheritdoc/>
        public Effect GetEffect(string effectId)
        {
            return effects.Get(effectId);
        }

        private static Dictionary<string, HeroClass> LoadHeroClasses()
        {
            var result = new Dictionary<string, HeroClass>();
            string directory = Path.Combine(AppContext.BaseDirectory, "Content", "Heroes");
            if (!Directory.Exists(directory))
                return result;

            var contents = Directory.GetFiles(directory, "*.bytes").OrderBy(path => path).Select(File.ReadAllText);
            var catalog = HeroCatalog.Load(contents);
            foreach (var classId in catalog.ClassIds)
            {
                HeroClass heroClass;
                if (catalog.TryGet(classId, out heroClass))
                    result[classId] = heroClass;
            }
            return result;
        }

        private static Dictionary<string, Quirk> LoadQuirks()
        {
            var result = new Dictionary<string, Quirk>();
            string path = Path.Combine(AppContext.BaseDirectory, "Content", "Quirks", "JsonQuirks.json");
            if (!File.Exists(path))
                return result;

            var data = JsonConvert.DeserializeObject<JsonQuirkData>(File.ReadAllText(path));
            if (data?.quirks == null)
                return result;

            foreach (var quirk in QuirkMapper.Parse(data.quirks))
                result[quirk.Id] = quirk;
            return result;
        }

        private static Dictionary<string, Buff> LoadBuffs()
        {
            var result = new Dictionary<string, Buff>();
            string path = Path.Combine(AppContext.BaseDirectory, "Content", "Buffs", "JsonBuffs.json");
            if (!File.Exists(path))
                return result;

            var data = JsonConvert.DeserializeObject<JsonBuffData>(File.ReadAllText(path));
            if (data?.buffs == null)
                return result;

            foreach (var content in BuffContentMapper.Parse(data.buffs))
            {
                AttributeType attribute = CharacterHelper.StringToAttributeType(content.AttributeTypeName);
                if (attribute == AttributeType.Undefined)
                    continue;

                var buff = new Buff(
                    CharacterHelper.StringToBuffType(content.StatType),
                    CharacterHelper.StringToBuffRule(content.RuleTypeName),
                    attribute,
                    content.Amount)
                {
                    Id = content.Id,
                    IsFalseRule = content.IsFalseRule,
                    SingleParam = content.RuleFloat,
                    StringParam = content.RuleString,
                };
                result[content.Id] = buff;
            }
            return result;
        }

        private static EffectCatalog LoadEffects()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Content", "Effects", "Effects.txt");
            return EffectCatalog.Load(File.Exists(path) ? File.ReadAllText(path) : string.Empty);
        }
    }
}