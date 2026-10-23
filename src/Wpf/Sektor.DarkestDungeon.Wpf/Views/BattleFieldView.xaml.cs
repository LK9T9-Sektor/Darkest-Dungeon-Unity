using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Sektor.DarkestDungeon.Wpf.Ui;
using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>
    /// Shared battle field: both sides of combatant cards on the common stage, used by the live
    /// battle screen and by the capture replay. Interaction is opt-in through the commands, so the
    /// replay simply leaves them unset and the cards only animate.
    /// </summary>
    public partial class BattleFieldView : UserControl
    {
        /// <summary>Identifies the <see cref="LeftUnits"/> dependency property.</summary>
        public static readonly DependencyProperty LeftUnitsProperty = DependencyProperty.Register(
            nameof(LeftUnits),
            typeof(IEnumerable),
            typeof(BattleFieldView),
            new PropertyMetadata(null));

        /// <summary>Identifies the <see cref="RightUnits"/> dependency property.</summary>
        public static readonly DependencyProperty RightUnitsProperty = DependencyProperty.Register(
            nameof(RightUnits),
            typeof(IEnumerable),
            typeof(BattleFieldView),
            new PropertyMetadata(null));

        /// <summary>Identifies the <see cref="SelectCommand"/> dependency property.</summary>
        public static readonly DependencyProperty SelectCommandProperty = DependencyProperty.Register(
            nameof(SelectCommand),
            typeof(ICommand),
            typeof(BattleFieldView),
            new PropertyMetadata(null));

        /// <summary>Identifies the <see cref="StatsCommand"/> dependency property.</summary>
        public static readonly DependencyProperty StatsCommandProperty = DependencyProperty.Register(
            nameof(StatsCommand),
            typeof(ICommand),
            typeof(BattleFieldView),
            new PropertyMetadata(null));

        /// <summary>Identifies the <see cref="InfoCommand"/> dependency property.</summary>
        public static readonly DependencyProperty InfoCommandProperty = DependencyProperty.Register(
            nameof(InfoCommand),
            typeof(ICommand),
            typeof(BattleFieldView),
            new PropertyMetadata(null));

        /// <summary>Identifies the <see cref="AnimationCompletedCommand"/> dependency property.</summary>
        public static readonly DependencyProperty AnimationCompletedCommandProperty = DependencyProperty.Register(
            nameof(AnimationCompletedCommand),
            typeof(ICommand),
            typeof(BattleFieldView),
            new PropertyMetadata(null));

        /// <summary>Identifies the <see cref="Overlay"/> dependency property.</summary>
        public static readonly DependencyProperty OverlayProperty = DependencyProperty.Register(
            nameof(Overlay),
            typeof(object),
            typeof(BattleFieldView),
            new PropertyMetadata(null));

        /// <summary>Identifies the <see cref="ActivityLabel"/> dependency property.</summary>
        public static readonly DependencyProperty ActivityLabelProperty = DependencyProperty.Register(
            nameof(ActivityLabel),
            typeof(string),
            typeof(BattleFieldView),
            new PropertyMetadata(string.Empty));

        /// <summary>Identifies the <see cref="ActivityTone"/> dependency property.</summary>
        public static readonly DependencyProperty ActivityToneProperty = DependencyProperty.Register(
            nameof(ActivityTone),
            typeof(SkillTone),
            typeof(BattleFieldView),
            new PropertyMetadata(SkillTone.Attack));

        /// <summary>Initializes a new instance of the <see cref="BattleFieldView"/> class.</summary>
        public BattleFieldView()
        {
            InitializeComponent();
        }

        /// <summary>Raised when the pointer enters a card.</summary>
        public event EventHandler<BattleFieldUnitEventArgs>? UnitHovered;

        /// <summary>Raised when the pointer leaves a card or a card is right-clicked.</summary>
        public event EventHandler? HoverCleared;

        /// <summary>Gets the stage element that hosts the cards; overlays draw over it.</summary>
        public FrameworkElement StageRoot
        {
            get { return Stage; }
        }

        /// <summary>Gets or sets the units on the player side (left).</summary>
        public IEnumerable? LeftUnits
        {
            get { return (IEnumerable?)GetValue(LeftUnitsProperty); }
            set { SetValue(LeftUnitsProperty, value); }
        }

        /// <summary>Gets or sets the units on the opponent side (right).</summary>
        public IEnumerable? RightUnits
        {
            get { return (IEnumerable?)GetValue(RightUnitsProperty); }
            set { SetValue(RightUnitsProperty, value); }
        }

        /// <summary>Gets or sets the command executed when a card is clicked.</summary>
        public ICommand? SelectCommand
        {
            get { return (ICommand?)GetValue(SelectCommandProperty); }
            set { SetValue(SelectCommandProperty, value); }
        }

        /// <summary>Gets or sets the command executed when a card is right-clicked.</summary>
        public ICommand? StatsCommand
        {
            get { return (ICommand?)GetValue(StatsCommandProperty); }
            set { SetValue(StatsCommandProperty, value); }
        }

        /// <summary>Gets or sets the command opening the buff/debuff table from a card.</summary>
        public ICommand? InfoCommand
        {
            get { return (ICommand?)GetValue(InfoCommandProperty); }
            set { SetValue(InfoCommandProperty, value); }
        }

        /// <summary>Gets or sets the command invoked when a card Spine animation completes.</summary>
        public ICommand? AnimationCompletedCommand
        {
            get { return (ICommand?)GetValue(AnimationCompletedCommandProperty); }
            set { SetValue(AnimationCompletedCommandProperty, value); }
        }

        /// <summary>Gets or sets the screen-owned layer drawn over the stage (target arrows).</summary>
        public object? Overlay
        {
            get { return GetValue(OverlayProperty); }
            set { SetValue(OverlayProperty, value); }
        }

        /// <summary>Gets or sets the label shown on the acting card badge (the skill name).</summary>
        public string ActivityLabel
        {
            get { return (string)GetValue(ActivityLabelProperty); }
            set { SetValue(ActivityLabelProperty, value); }
        }

        /// <summary>Gets or sets the tone that colors the acting and target badges.</summary>
        public SkillTone ActivityTone
        {
            get { return (SkillTone)GetValue(ActivityToneProperty); }
            set { SetValue(ActivityToneProperty, value); }
        }

        private void OnSlotMouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is IBattleFieldUnit unit)
                UnitHovered?.Invoke(this, new BattleFieldUnitEventArgs(unit));
        }

        private void OnSlotMouseLeave(object sender, MouseEventArgs e)
        {
            HoverCleared?.Invoke(this, EventArgs.Empty);
        }

        private void OnSlotRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            HoverCleared?.Invoke(this, EventArgs.Empty);
            if (sender is not FrameworkElement element || element.DataContext is not IBattleFieldUnit unit)
                return;

            if (StatsCommand != null && StatsCommand.CanExecute(unit))
            {
                StatsCommand.Execute(unit);
                e.Handled = true;
            }
        }
    }
}
