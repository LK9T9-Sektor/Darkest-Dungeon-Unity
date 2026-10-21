using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Reusable two-slot trinket editor with the TRINKETS heading.</summary>
    public partial class HeroTrinketsEditorView : UserControl
    {
        /// <summary>Identifies the <see cref="ItemsSource"/> dependency property.</summary>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(HeroTrinketsEditorView),
            new PropertyMetadata(null));

        /// <summary>Initializes a new instance of the <see cref="HeroTrinketsEditorView"/> class.</summary>
        public HeroTrinketsEditorView()
        {
            InitializeComponent();
        }

        /// <summary>Gets or sets the trinket slot view models.</summary>
        public IEnumerable? ItemsSource
        {
            get => (IEnumerable?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }
    }
}
