using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Shared hero slot selection list used by both lobbies.</summary>
    public partial class HeroSlotsPanel : UserControl
    {
        /// <summary>Identifies the <see cref="Slots"/> dependency property.</summary>
        public static readonly DependencyProperty SlotsProperty = DependencyProperty.Register(
            nameof(Slots),
            typeof(IEnumerable),
            typeof(HeroSlotsPanel),
            new PropertyMetadata(null));

        /// <summary>Initializes a new instance of the <see cref="HeroSlotsPanel"/> class.</summary>
        public HeroSlotsPanel()
        {
            InitializeComponent();
        }

        /// <summary>Gets or sets the hero slot collection.</summary>
        public IEnumerable Slots
        {
            get { return (IEnumerable)GetValue(SlotsProperty); }
            set { SetValue(SlotsProperty, value); }
        }
    }
}
