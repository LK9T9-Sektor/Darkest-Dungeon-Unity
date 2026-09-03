using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Reusable skill square: an icon slot with the skill name below and a structured
    /// tooltip on hover. Used in the bottom-left skill tray and in the character-info sheet.</summary>
    public partial class SkillSquareView : UserControl
    {
        /// <summary>Identifies the <see cref="SelectCommand"/> dependency property.</summary>
        public static readonly DependencyProperty SelectCommandProperty =
            DependencyProperty.Register(
                nameof(SelectCommand),
                typeof(ICommand),
                typeof(SkillSquareView),
                new PropertyMetadata(null, OnSelectCommandChanged));

        /// <summary>Identifies the <see cref="CommandParameter"/> dependency property.</summary>
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(
                nameof(CommandParameter),
                typeof(object),
                typeof(SkillSquareView),
                new PropertyMetadata(null, OnCommandParameterChanged));

        /// <summary>Identifies the <see cref="ShowName"/> dependency property.</summary>
        public static readonly DependencyProperty ShowNameProperty =
            DependencyProperty.Register(
                nameof(ShowName),
                typeof(bool),
                typeof(SkillSquareView),
                new PropertyMetadata(true, OnShowNameChanged));

        /// <summary>Gets or sets the command invoked when the skill square is clicked (tray only).</summary>
        public ICommand? SelectCommand
        {
            get { return (ICommand?)GetValue(SelectCommandProperty); }
            set { SetValue(SelectCommandProperty, value); }
        }

        /// <summary>Gets or sets the parameter passed to <see cref="SelectCommand"/>.</summary>
        public object? CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        /// <summary>Gets or sets a value indicating whether the name label is shown below the icon
        /// (default true). Turned off for the compact floating badge.</summary>
        public bool ShowName
        {
            get { return (bool)GetValue(ShowNameProperty); }
            set { SetValue(ShowNameProperty, value); }
        }

        /// <summary>Initializes a new instance of the <see cref="SkillSquareView"/> class.</summary>
        public SkillSquareView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private static void OnSelectCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SkillSquareView)d).WireButton();
        }

        private static void OnCommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SkillSquareView)d).WireButton();
        }

        private static void OnShowNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SkillSquareView)d).WireNameVisibility();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            WireButton();
        }

        private void WireNameVisibility()
        {
            NameLabel.Visibility = ShowName ? Visibility.Visible : Visibility.Collapsed;
        }

        private void WireButton()
        {
            // Without a SelectCommand the square renders as a read-only inspection slot
            // (character-info sheet, floating badge); with one it acts as the clickable skill tray
            // button. Read-only slots stay hit-testable (and enabled) so their hover tooltip works,
            // but carry no command, so a click does nothing.
            SkillButton.Command = SelectCommand;
            SkillButton.CommandParameter = CommandParameter;
            bool clickable = SelectCommand != null;
            SkillButton.IsHitTestVisible = true;
            SkillButton.IsEnabled = clickable ? (DataContext is DuelSkillViewModel skill && skill.IsUsable) : true;
            WireNameVisibility();
        }
    }
}
