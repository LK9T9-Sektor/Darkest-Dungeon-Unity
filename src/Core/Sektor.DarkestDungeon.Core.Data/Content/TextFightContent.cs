using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Content.Character;
using Sektor.DarkestDungeon.Core.Duel;
using Sektor.DarkestDungeon.Core.Data.Catalogs;

namespace Sektor.DarkestDungeon.Core.Data.Content
{
    /// <summary>
    /// Campaigned content source for the duel fight runner, backed by the catalogs produced by
    /// <see cref="Readers.GameDataReader"/>. Used by the automated fight tests and by widgets that
    /// lack a dedicated content loader.
    /// </summary>
    public sealed class TextFightContent : IDuelContent
    {
        private readonly HeroCatalog heroes;
        private readonly MonsterCatalog monsters;
        private readonly MonsterBrainCatalog brains;
        private readonly BuffCatalog buffs;
        private readonly QuirkCatalog quirks;
        private readonly EffectCatalog effects;
        private readonly IReadOnlyList<Trait> afflictions;
        private readonly IReadOnlyList<Trait> virtues;

        /// <summary>Initializes a new instance of the <see cref="TextFightContent"/> class.</summary>
        /// <param name="heroes">The hero class catalog.</param>
        /// <param name="monsters">The monster class catalog.</param>
        /// <param name="brains">The monster brain catalog.</param>
        /// <param name="buffs">The buff catalog.</param>
        /// <param name="quirks">The quirk catalog.</param>
        /// <param name="effects">The effect catalog.</param>
        /// <param name="afflictions">The affliction traits.</param>
        /// <param name="virtues">The virtue traits.</param>
        public TextFightContent(
            HeroCatalog heroes,
            MonsterCatalog monsters,
            MonsterBrainCatalog brains,
            BuffCatalog buffs,
            QuirkCatalog quirks,
            EffectCatalog effects,
            IReadOnlyList<Trait> afflictions,
            IReadOnlyList<Trait> virtues)
        {
            this.heroes = heroes;
            this.monsters = monsters;
            this.brains = brains;
            this.buffs = buffs;
            this.quirks = quirks;
            this.effects = effects;
            this.afflictions = afflictions;
            this.virtues = virtues;
        }

        /// <inheritdoc/>
        public HeroClass GetHeroClass(string classId)
        {
            if (classId == null)
                return null;

            HeroClass heroClass;
            return heroes.TryGet(classId, out heroClass) ? heroClass : null;
        }

        /// <inheritdoc/>
        public MonsterClass GetMonsterClass(string monsterId)
        {
            if (monsterId == null)
                return null;

            MonsterClass monsterClass;
            return monsters.TryGet(monsterId, out monsterClass) ? monsterClass : null;
        }

        /// <inheritdoc/>
        public MonsterBrain GetMonsterBrain(string brainId)
        {
            if (brainId == null)
                return null;

            MonsterBrain brain;
            return brains.TryGet(brainId, out brain) ? brain : null;
        }

        /// <inheritdoc/>
        public Quirk GetQuirk(string quirkId)
        {
            return quirks.Get(quirkId);
        }

        /// <inheritdoc/>
        public Buff GetBuff(string buffId)
        {
            return buffs.Get(buffId);
        }

        /// <inheritdoc/>
        public Effect GetEffect(string effectId)
        {
            return effectId == null ? null : effects.Get(effectId);
        }

        /// <inheritdoc/>
        public IReadOnlyList<Trait> GetAfflictions()
        {
            return afflictions;
        }

        /// <inheritdoc/>
        public IReadOnlyList<Trait> GetVirtues()
        {
            return virtues;
        }
    }
}