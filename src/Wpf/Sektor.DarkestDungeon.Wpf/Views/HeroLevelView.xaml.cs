using System.Windows;
using System.Windows.Controls;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Reusable hero level medallion: a rounded black frame with a circular badge and number.</summary>
    public partial class HeroLevelView : UserControl
    {
        /// <summary>Identifies the <see cref="Level"/> dependency property.</summary>
        public static readonly DependencyProperty LevelProperty = DependencyProperty.Register(
            nameof(Level),
            typeof(int),
            typeof(HeroLevelView),
            new PropertyMetadata(1));

        /// <summary>Initializes a new instance of the <see cref="HeroLevelView"/> class.</summary>
        public HeroLevelView()
        {
            InitializeComponent();
        }

        /// <summary>Gets or sets the hero level shown inside the badge.</summary>
        public int Level
        {
            get => (int)GetValue(LevelProperty);
            set => SetValue(LevelProperty, value);
        }
    }
}
