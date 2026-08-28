using System.Windows;
using System.Windows.Controls;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Hero stat sheet view.</summary>
    public partial class HeroStatsView : UserControl
    {
        /// <summary>Identifies the <see cref="ShowFullDetails"/> dependency property.</summary>
        public static readonly DependencyProperty ShowFullDetailsProperty =
            DependencyProperty.Register(
                nameof(ShowFullDetails),
                typeof(bool),
                typeof(HeroStatsView),
                new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value indicating whether the full details sections
        /// (skills, resistances, quirks) are shown. The left hero panel keeps them hidden.
        /// </summary>
        public bool ShowFullDetails
        {
            get { return (bool)GetValue(ShowFullDetailsProperty); }
            set { SetValue(ShowFullDetailsProperty, value); }
        }

        /// <summary>Initializes a new instance of the <see cref="HeroStatsView"/> class.</summary>
        public HeroStatsView()
        {
            InitializeComponent();
        }
    }
}
