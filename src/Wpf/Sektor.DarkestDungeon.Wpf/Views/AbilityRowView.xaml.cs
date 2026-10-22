using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Reusable row of skill squares with an optional selection command, configurable icon
    /// size, and optional name/level labels.</summary>
    public partial class AbilityRowView : UserControl
    {
        /// <summary>Identifies the <see cref="ItemsSource"/> dependency property.</summary>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(AbilityRowView),
            new PropertyMetadata(null));

        /// <summary>Identifies the <see cref="SelectCommand"/> dependency property.</summary>
        public static readonly DependencyProperty SelectCommandProperty = DependencyProperty.Register(
            nameof(SelectCommand),
            typeof(ICommand),
            typeof(AbilityRowView),
            new PropertyMetadata(null));

        /// <summary>Identifies the <see cref="IconSize"/> dependency property.</summary>
        public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(
            nameof(IconSize),
            typeof(double),
            typeof(AbilityRowView),
            new PropertyMetadata(42.0));

        /// <summary>Identifies the <see cref="ShowNames"/> dependency property.</summary>
        public static readonly DependencyProperty ShowNamesProperty = DependencyProperty.Register(
            nameof(ShowNames),
            typeof(bool),
            typeof(AbilityRowView),
            new PropertyMetadata(true));

        /// <summary>Identifies the <see cref="ShowLevels"/> dependency property.</summary>
        public static readonly DependencyProperty ShowLevelsProperty = DependencyProperty.Register(
            nameof(ShowLevels),
            typeof(bool),
            typeof(AbilityRowView),
            new PropertyMetadata(false));

        /// <summary>Initializes a new instance of the <see cref="AbilityRowView"/> class.</summary>
        public AbilityRowView()
        {
            InitializeComponent();
        }

        /// <summary>Gets or sets the skill view models to render.</summary>
        public IEnumerable? ItemsSource
        {
            get => (IEnumerable?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        /// <summary>Gets or sets the command invoked when a skill square is clicked; null makes the row read-only.</summary>
        public ICommand? SelectCommand
        {
            get => (ICommand?)GetValue(SelectCommandProperty);
            set => SetValue(SelectCommandProperty, value);
        }

        /// <summary>Gets or sets the rendered icon area size in device independent pixels (default 42).</summary>
        public double IconSize
        {
            get => (double)GetValue(IconSizeProperty);
            set => SetValue(IconSizeProperty, value);
        }

        /// <summary>Gets or sets a value indicating whether the skill name is shown under each icon.</summary>
        public bool ShowNames
        {
            get => (bool)GetValue(ShowNamesProperty);
            set => SetValue(ShowNamesProperty, value);
        }

        /// <summary>Gets or sets a value indicating whether the skill level number is shown under each icon.</summary>
        public bool ShowLevels
        {
            get => (bool)GetValue(ShowLevelsProperty);
            set => SetValue(ShowLevelsProperty, value);
        }
    }
}
