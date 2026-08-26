namespace Sektor.DarkestDungeon.Core.Combat.Skills
{
    /// <summary>Move component for skills that change position.</summary>
    public class MoveComponent
    {
        /// <summary>Gets the pushback amount.</summary>
        public int Pushback { get; }

        /// <summary>Gets the pullforward amount.</summary>
        public int Pullforward { get; }

        /// <summary>Initializes a new instance of the <see cref="MoveComponent"/> class.</summary>
        /// <param name="push">Pushback amount.</param>
        /// <param name="pull">Pullforward amount.</param>
        public MoveComponent(int push, int pull)
        {
            Pushback = push;
            Pullforward = pull;
        }
    }
}
