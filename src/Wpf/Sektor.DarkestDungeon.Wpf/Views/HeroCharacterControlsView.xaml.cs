using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Reusable bottom-left character controls: portrait/class arrows/position and skin selector.</summary>
    public partial class HeroCharacterControlsView : UserControl
    {
        /// <summary>Identifies the <see cref="Portrait"/> dependency property.</summary>
        public static readonly DependencyProperty PortraitProperty = DependencyProperty.Register(
            nameof(Portrait), typeof(ImageSource), typeof(HeroCharacterControlsView), new PropertyMetadata(null));

        /// <summary>Identifies the <see cref="Position"/> dependency property.</summary>
        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register(
            nameof(Position), typeof(int), typeof(HeroCharacterControlsView), new PropertyMetadata(0));

        /// <summary>Identifies the <see cref="SkinId"/> dependency property.</summary>
        public static readonly DependencyProperty SkinIdProperty = DependencyProperty.Register(
            nameof(SkinId), typeof(int), typeof(HeroCharacterControlsView), new PropertyMetadata(1));

        /// <summary>Identifies the <see cref="PreviousCommand"/> dependency property.</summary>
        public static readonly DependencyProperty PreviousCommandProperty = DependencyProperty.Register(
            nameof(PreviousCommand), typeof(ICommand), typeof(HeroCharacterControlsView), new PropertyMetadata(null));

        /// <summary>Identifies the <see cref="NextCommand"/> dependency property.</summary>
        public static readonly DependencyProperty NextCommandProperty = DependencyProperty.Register(
            nameof(NextCommand), typeof(ICommand), typeof(HeroCharacterControlsView), new PropertyMetadata(null));

        /// <summary>Identifies the <see cref="SelectSkinCommand"/> dependency property.</summary>
        public static readonly DependencyProperty SelectSkinCommandProperty = DependencyProperty.Register(
            nameof(SelectSkinCommand), typeof(ICommand), typeof(HeroCharacterControlsView), new PropertyMetadata(null));

        /// <summary>Initializes a new instance of the <see cref="HeroCharacterControlsView"/> class.</summary>
        public HeroCharacterControlsView()
        {
            InitializeComponent();
        }

        /// <summary>Gets or sets the portrait image.</summary>
        public ImageSource? Portrait
        {
            get => (ImageSource?)GetValue(PortraitProperty);
            set => SetValue(PortraitProperty, value);
        }

        /// <summary>Gets or sets the 1-based formation position.</summary>
        public int Position
        {
            get => (int)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }

        /// <summary>Gets or sets the current skin id.</summary>
        public int SkinId
        {
            get => (int)GetValue(SkinIdProperty);
            set => SetValue(SkinIdProperty, value);
        }

        /// <summary>Gets or sets the previous-class command.</summary>
        public ICommand? PreviousCommand
        {
            get => (ICommand?)GetValue(PreviousCommandProperty);
            set => SetValue(PreviousCommandProperty, value);
        }

        /// <summary>Gets or sets the next-class command.</summary>
        public ICommand? NextCommand
        {
            get => (ICommand?)GetValue(NextCommandProperty);
            set => SetValue(NextCommandProperty, value);
        }

        /// <summary>Gets or sets the skin-change command.</summary>
        public ICommand? SelectSkinCommand
        {
            get => (ICommand?)GetValue(SelectSkinCommandProperty);
            set => SetValue(SelectSkinCommandProperty, value);
        }
    }
}
