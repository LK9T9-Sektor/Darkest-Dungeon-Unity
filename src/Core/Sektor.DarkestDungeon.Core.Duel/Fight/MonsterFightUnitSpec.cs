namespace Sektor.DarkestDungeon.Core.Duel.Fight
{
    /// <summary>A monster fighter defined by its monster class id.</summary>
    public sealed class MonsterFightUnitSpec : FightUnitSpec
    {
        /// <summary>Initializes a new instance of the <see cref="MonsterFightUnitSpec"/> class.</summary>
        /// <param name="monsterId">The monster class id.</param>
        public MonsterFightUnitSpec(string monsterId)
        {
            MonsterId = monsterId;
        }

        /// <summary>Gets the monster class id.</summary>
        public string MonsterId { get; }
    }
}