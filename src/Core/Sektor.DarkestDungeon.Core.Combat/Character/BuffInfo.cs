using Sektor.DarkestDungeon.Core.Combat.Mechanics;

namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>An applied buff instance with its duration and source.</summary>
    public class BuffInfo
    {
        /// <summary>Gets the buff definition.</summary>
        public Buff Buff { get; private set; }

        /// <summary>Gets the duration type.</summary>
        public BuffDurationType DurationType { get; private set; }

        /// <summary>Gets the source type.</summary>
        public BuffSourceType SourceType { get; private set; }

        /// <summary>Gets or sets the remaining duration.</summary>
        public int Duration { get; set; }

        /// <summary>Gets or sets a value indicating whether the buff is applied.</summary>
        public bool IsApplied { get; set; }

        /// <summary>Gets the effective modifier value (with override if set).</summary>
        public float ModifierValue { get { return Approximately(OverridenValue, 0.0f) ? Buff.ModifierValue : OverridenValue; } }

        private float OverridenValue { get; set; }

        /// <summary>Initializes a new instance of the <see cref="BuffInfo"/> class.</summary>
        public BuffInfo()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="BuffInfo"/> class.</summary>
        /// <param name="buff">The buff definition.</param>
        /// <param name="durationType">The duration type.</param>
        /// <param name="sourceType">The source type.</param>
        /// <param name="duration">The duration amount.</param>
        public BuffInfo(Buff buff, BuffDurationType durationType, BuffSourceType sourceType, int duration = 1)
        {
            Buff = buff;
            DurationType = durationType;
            SourceType = sourceType;
            Duration = duration;
        }

        /// <summary>Initializes a new instance of the <see cref="BuffInfo"/> class.</summary>
        /// <param name="buff">The buff definition.</param>
        /// <param name="sourceType">The source type.</param>
        public BuffInfo(Buff buff, BuffSourceType sourceType)
        {
            Buff = buff;
            DurationType = buff.DurationType;
            SourceType = sourceType;
            Duration = buff.DurationAmount;
        }

        /// <summary>Initializes a new instance of the <see cref="BuffInfo"/> class.</summary>
        /// <param name="buff">The buff definition.</param>
        /// <param name="overridenValue">The overridden modifier value.</param>
        /// <param name="sourceType">The source type.</param>
        public BuffInfo(Buff buff, float overridenValue, BuffSourceType sourceType)
        {
            Buff = buff;
            OverridenValue = overridenValue;
            DurationType = buff.DurationType;
            SourceType = sourceType;
            Duration = buff.DurationAmount;
        }

        private static bool Approximately(float a, float b)
        {
            return System.Math.Abs(a - b) < 0.000001f;
        }
    }
}