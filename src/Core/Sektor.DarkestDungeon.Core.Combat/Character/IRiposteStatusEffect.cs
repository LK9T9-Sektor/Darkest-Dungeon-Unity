using Sektor.DarkestDungeon.Core.Combat.Mechanics;

namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Abstraction of the riposte status effect.</summary>
    public interface IRiposteStatusEffect : IStatusEffect
    {
        /// <summary>Gets or sets the riposte duration.</summary>
        int RiposteDuration { get; set; }

        /// <summary>Gets or sets the duration type.</summary>
        DurationType DurationType { get; set; }
    }
}