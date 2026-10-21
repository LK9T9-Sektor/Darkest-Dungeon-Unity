using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Reusable hero card: a roster portrait in the dark frame with the hero name under it and
    /// optional class-cycling arrows around the portrait. The host supplies the portrait, the caption,
    /// the arrow commands and (optionally) the portrait click command; without a click command the
    /// portrait stays read-only.</summary>
    public partial class HeroPortraitView : UserControl
    {
        /// <summary>Identifies the <see cref="Portrait"/> dependency property.</summary>
        public static readonly DependencyProperty PortraitProperty = DependencyProperty.Register(
            nameof(Portrait),
            typeof(ImageSource),
            typeof(HeroPortraitView),
            new PropertyMetadata(null));

        /// <summary>Identifies the <see cref="Caption"/> dependency property.</summary>
        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register(
            nameof(Caption),
            typeof(string),
            typeof(HeroPortraitView),
            new PropertyMetadata(string.Empty));

        /// <summary>Identifies the <see cref="PortraitSize"/> dependency property.</summary>
        public static readonly DependencyProperty PortraitSizeProperty = DependencyProperty.Register(
            nameof(PortraitSize),
            typeof(double),
            typeof(HeroPortraitView),
            new PropertyMetadata(96.0));

        /// <summary>Identifies the <see cref="ShowArrows"/> dependency property.</summary>
        public static readonly DependencyProperty ShowArrowsProperty = DependencyProperty.Register(
            nameof(ShowArrows),
            typeof(bool),
            typeof(HeroPortraitView),
            new PropertyMetadata(false));

        /// <summary>Identifies the <see cref="ArrowSize"/> dependency property.</summary>
        public static readonly DependencyProperty ArrowSizeProperty = DependencyProperty.Register(
            nameof(ArrowSize),
            typeof(double),
            typeof(HeroPortraitView),
            new PropertyMetadata(40.0));

        /// <summary>Identifies the <see cref="PreviousCommand"/> dependency property.</summary>
        public static readonly DependencyProperty PreviousCommandProperty = DependencyProperty.Register(
            nameof(PreviousCommand),
            typeof(ICommand),
            typeof(HeroPortraitView),
            new PropertyMetadata(null));

        /// <summary>Identifies the <see cref="NextCommand"/> dependency property.</summary>
        public static readonly DependencyProperty NextCommandProperty = DependencyProperty.Register(
            nameof(NextCommand),
            typeof(ICommand),
            typeof(HeroPortraitView),
            new PropertyMetadata(null));

        /// <summary>Identifies the <see cref="SelectCommand"/> dependency property.</summary>
        public static readonly DependencyProperty SelectCommandProperty = DependencyProperty.Register(
            nameof(SelectCommand),
            typeof(ICommand),
            typeof(HeroPortraitView),
            new PropertyMetadata(null, OnSelectCommandChanged));

        /// <summary>Initializes a new instance of the <see cref="HeroPortraitView"/> class.</summary>
        public HeroPortraitView()
        {
            InitializeComponent();
        }

        /// <summary>Gets or sets the portrait image; null shows the "?" placeholder.</summary>
        public ImageSource? Portrait
        {
            get => (ImageSource?)GetValue(PortraitProperty);
            set => SetValue(PortraitProperty, value);
        }

        /// <summary>Gets or sets the hero name shown under the portrait; an empty value hides the caption.</summary>
        public string Caption
        {
            get => (string)GetValue(CaptionProperty);
            set => SetValue(CaptionProperty, value);
        }

        /// <summary>Gets or sets the width and height of the square portrait frame in device independent pixels.</summary>
        public double PortraitSize
        {
            get => (double)GetValue(PortraitSizeProperty);
            set => SetValue(PortraitSizeProperty, value);
        }

        /// <summary>Gets or sets a value indicating whether the class-cycling arrows are shown around
        /// the portrait. False keeps the card read-only (battle HUD, info panels).</summary>
        public bool ShowArrows
        {
            get => (bool)GetValue(ShowArrowsProperty);
            set => SetValue(ShowArrowsProperty, value);
        }

        /// <summary>Gets or sets the width and height of the class-cycling arrow buttons in device
        /// independent pixels (default 40).</summary>
        public double ArrowSize
        {
            get => (double)GetValue(ArrowSizeProperty);
            set => SetValue(ArrowSizeProperty, value);
        }

        /// <summary>Gets or sets the command that cycles to the previous hero (the left arrow).</summary>
        public ICommand? PreviousCommand
        {
            get => (ICommand?)GetValue(PreviousCommandProperty);
            set => SetValue(PreviousCommandProperty, value);
        }

        /// <summary>Gets or sets the command that cycles to the next hero (the right arrow).</summary>
        public ICommand? NextCommand
        {
            get => (ICommand?)GetValue(NextCommandProperty);
            set => SetValue(NextCommandProperty, value);
        }

        /// <summary>Gets or sets the command invoked by clicking the portrait; null makes the portrait
        /// read-only (no hand cursor, no click handling).</summary>
        public ICommand? SelectCommand
        {
            get => (ICommand?)GetValue(SelectCommandProperty);
            set => SetValue(SelectCommandProperty, value);
        }

        private static void OnSelectCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (HeroPortraitView)d;
            if (view.PortraitButton == null)
                return;

            bool clickable = e.NewValue != null;
            view.PortraitButton.Cursor = clickable ? Cursors.Hand : Cursors.Arrow;
            view.PortraitButton.IsHitTestVisible = clickable;
        }
    }
}
