using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Reusable hero card: a roster portrait in a dark frame with the hero name under it.</summary>
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
    }
}
