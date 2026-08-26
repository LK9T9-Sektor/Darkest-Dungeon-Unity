using Sektor.DarkestDungeon.Core.Combat.Mechanics;

namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Abstraction of the marked status effect.</summary>
    public interface IMarkStatusEffect : IStatusEffect
    {
        /// <summary>Gets or sets the mark duration.</summary>
        int MarkDuration { get; set; }

        /// <summary>Gets or sets the duration type.</summary>
        DurationType DurationType { get; set; }
    }
}