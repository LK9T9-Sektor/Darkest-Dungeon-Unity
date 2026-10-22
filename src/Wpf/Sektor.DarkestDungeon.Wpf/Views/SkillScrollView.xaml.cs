using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Reusable skill grid that shows at most <see cref="MaxRows"/> rows and scrolls
    /// vertically when the items do not fit. An empty list still occupies one row so the hosting
    /// section keeps a stable height. The item chrome is delegated to <see cref="AbilityRowView"/>.</summary>
    public partial class SkillScrollView : UserControl
    {
        private const double IconChrome = 8.0;
        private const double NameRowHeight = 22.0;
        private const double LevelRowHeight = 18.0;
        private const double MinIconSize = 16.0;

        /// <summary>Identifies the <see cref="ItemsSource"/> dependency property.</summary>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(SkillScrollView),
            new PropertyMetadata(null, OnLayoutPropertyChanged));

        /// <summary>Identifies the <see cref="SelectCommand"/> dependency property.</summary>
        public static readonly DependencyProperty SelectCommandProperty = DependencyProperty.Register(
            nameof(SelectCommand),
            typeof(ICommand),
            typeof(SkillScrollView),
            new PropertyMetadata(null));

        /// <summary>Identifies the <see cref="IconSize"/> dependency property.</summary>
        public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(
            nameof(IconSize),
            typeof(double),
            typeof(SkillScrollView),
            new PropertyMetadata(42.0, OnLayoutPropertyChanged));

        /// <summary>Identifies the <see cref="ShowNames"/> dependency property.</summary>
        public static readonly DependencyProperty ShowNamesProperty = DependencyProperty.Register(
            nameof(ShowNames),
            typeof(bool),
            typeof(SkillScrollView),
            new PropertyMetadata(true, OnLayoutPropertyChanged));

        /// <summary>Identifies the <see cref="ShowLevels"/> dependency property.</summary>
        public static readonly DependencyProperty ShowLevelsProperty = DependencyProperty.Register(
            nameof(ShowLevels),
            typeof(bool),
            typeof(SkillScrollView),
            new PropertyMetadata(false, OnLayoutPropertyChanged));

        /// <summary>Identifies the <see cref="MaxRows"/> dependency property.</summary>
        public static readonly DependencyProperty MaxRowsProperty = DependencyProperty.Register(
            nameof(MaxRows),
            typeof(int),
            typeof(SkillScrollView),
            new PropertyMetadata(2, OnLayoutPropertyChanged));

        /// <summary>Identifies the <see cref="IsReadOnly"/> dependency property.</summary>
        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(SkillScrollView),
            new PropertyMetadata(false));

        /// <summary>Initializes a new instance of the <see cref="SkillScrollView"/> class.</summary>
        public SkillScrollView()
        {
            InitializeComponent();
            UpdateRowBounds();
        }

        /// <summary>Gets or sets the skill tile view models.</summary>
        public IEnumerable? ItemsSource
        {
            get => (IEnumerable?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        /// <summary>Gets or sets the command invoked when a skill tile is clicked.</summary>
        public ICommand? SelectCommand
        {
            get => (ICommand?)GetValue(SelectCommandProperty);
            set => SetValue(SelectCommandProperty, value);
        }

        /// <summary>Gets or sets the rendered icon area size in device independent pixels.</summary>
        public double IconSize
        {
            get => (double)GetValue(IconSizeProperty);
            set => SetValue(IconSizeProperty, value);
        }

        /// <summary>Gets or sets a value indicating whether the name label is shown under each icon.</summary>
        public bool ShowNames
        {
            get => (bool)GetValue(ShowNamesProperty);
            set => SetValue(ShowNamesProperty, value);
        }

        /// <summary>Gets or sets a value indicating whether the level number is shown under each icon.</summary>
        public bool ShowLevels
        {
            get => (bool)GetValue(ShowLevelsProperty);
            set => SetValue(ShowLevelsProperty, value);
        }

        /// <summary>Gets or sets the maximum number of visible rows (at least one).</summary>
        public int MaxRows
        {
            get => (int)GetValue(MaxRowsProperty);
            set => SetValue(MaxRowsProperty, value);
        }

        /// <summary>Gets or sets a value indicating whether the tiles ignore their selection command.</summary>
        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SkillScrollView)d).UpdateRowBounds();
        }

        private void UpdateRowBounds()
        {
            if (Scroller == null)
                return;

            double rowHeight = Math.Max(MinIconSize, IconSize) + IconChrome;
            if (ShowNames)
                rowHeight += NameRowHeight;
            if (ShowLevels)
                rowHeight += LevelRowHeight;

            int rows = Math.Max(1, MaxRows);
            Scroller.MinHeight = rowHeight;
            Scroller.MaxHeight = rowHeight * rows;
        }
    }
}
