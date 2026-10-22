using System;
using System.ComponentModel;
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
        private const double DefaultIconSize = 42.0;
        private const double IconChrome = 8.0;
        private const double NameExtraWidth = 8.0;
        private const double MinIconSize = 16.0;

        private INotifyPropertyChanged? observedTile;

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

        /// <summary>Identifies the <see cref="IconSize"/> dependency property.</summary>
        public static readonly DependencyProperty IconSizeProperty =
            DependencyProperty.Register(
                nameof(IconSize),
                typeof(double),
                typeof(SkillSquareView),
                new PropertyMetadata(DefaultIconSize, OnIconSizeChanged));

        /// <summary>Identifies the <see cref="ShowLevel"/> dependency property.</summary>
        public static readonly DependencyProperty ShowLevelProperty =
            DependencyProperty.Register(
                nameof(ShowLevel),
                typeof(bool),
                typeof(SkillSquareView),
                new PropertyMetadata(false, OnShowLevelChanged));

        /// <summary>Identifies the <see cref="IsReadOnly"/> dependency property.</summary>
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(
                nameof(IsReadOnly),
                typeof(bool),
                typeof(SkillSquareView),
                new PropertyMetadata(false, OnIsReadOnlyChanged));

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

        /// <summary>Gets or sets the rendered icon area size in device independent pixels (default 42).
        /// The square adds an 8 px frame around the image, so the default keeps the legacy 50 px tray
        /// button, while 72 renders the bundled 72x72 ability icons 1:1.</summary>
        public double IconSize
        {
            get { return (double)GetValue(IconSizeProperty); }
            set { SetValue(IconSizeProperty, value); }
        }

        /// <summary>Gets or sets a value indicating whether the skill level number is shown under the
        /// icon (default false). The level is read from the bound tile's <c>Level</c> property.</summary>
        public bool ShowLevel
        {
            get { return (bool)GetValue(ShowLevelProperty); }
            set { SetValue(ShowLevelProperty, value); }
        }

        /// <summary>Gets or sets a value indicating whether the tile ignores its selection command.</summary>
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        /// <summary>Initializes a new instance of the <see cref="SkillSquareView"/> class.</summary>
        public SkillSquareView()
        {
            InitializeComponent();
            ApplyIconSize();
            DataContextChanged += OnDataContextChanged;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
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
            var view = (SkillSquareView)d;
            view.WireNameVisibility();
            view.ApplyIconSize();
        }

        private static void OnIconSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SkillSquareView)d).ApplyIconSize();
        }

        private static void OnShowLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SkillSquareView)d).WireLevelVisibility();
        }

        private static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SkillSquareView)d).WireButton();
        }

        private void ApplyIconSize()
        {
            double iconSize = Math.Max(MinIconSize, IconSize);
            SkillButton.Width = iconSize + IconChrome;
            SkillButton.Height = iconSize + IconChrome;
            RootPanel.Width = iconSize + IconChrome + (ShowName ? NameExtraWidth : 0);
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsLoaded)
                ObserveTile(e.NewValue);
            WireButton();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ObserveTile(DataContext);
            WireButton();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ObserveTile(null);
        }

        private void ObserveTile(object? dataContext)
        {
            if (observedTile != null)
                observedTile.PropertyChanged -= OnTilePropertyChanged;

            observedTile = dataContext as INotifyPropertyChanged;
            if (observedTile != null)
                observedTile.PropertyChanged += OnTilePropertyChanged;
        }

        private void OnTilePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ISkillTileViewModel.IsUsable))
                WireButton();
        }

        private void WireNameVisibility()
        {
            NameLabel.Visibility = ShowName ? Visibility.Visible : Visibility.Collapsed;
        }

        private void WireLevelVisibility()
        {
            LevelLabel.Visibility = ShowLevel ? Visibility.Visible : Visibility.Collapsed;
        }

        private void WireButton()
        {
            // Without a SelectCommand the square renders as a read-only inspection slot
            // (character-info sheet, floating badge); with one it acts as the clickable skill tray
            // button. Read-only slots stay hit-testable (and enabled) so their hover tooltip works,
            // but carry no command, so a click does nothing. Clickable slots follow the shared
            // ISkillTileViewModel availability flag, and are refreshed when the item raises
            // PropertyChanged for IsUsable (e.g. a lobby slot reaching its skill limit).
            SkillButton.Command = IsReadOnly ? null : SelectCommand;
            SkillButton.CommandParameter = CommandParameter;
            bool clickable = !IsReadOnly && SelectCommand != null;
            bool usable = DataContext is ISkillTileViewModel tile && tile.IsUsable;
            SkillButton.IsHitTestVisible = true;
            SkillButton.IsEnabled = !clickable || usable;
            WireNameVisibility();
            WireLevelVisibility();
        }
    }
}
