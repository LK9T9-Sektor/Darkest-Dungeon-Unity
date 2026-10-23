using System;

using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Carries the unit a battle-field card is currently hovered over.</summary>
    public sealed class BattleFieldUnitEventArgs : EventArgs
    {
        /// <summary>Initializes a new instance of the <see cref="BattleFieldUnitEventArgs"/> class.</summary>
        /// <param name="unit">The hovered unit.</param>
        public BattleFieldUnitEventArgs(IBattleFieldUnit unit)
        {
            Unit = unit;
        }

        /// <summary>Gets the hovered unit.</summary>
        public IBattleFieldUnit Unit { get; }
    }
}
