using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Reusable square skin selector showing the current skin id and invoking a change command.</summary>
    public partial class HeroSkinView : UserControl
    {
        /// <summary>Identifies the <see cref="SkinId"/> dependency property.</summary>
        public static readonly DependencyProperty SkinIdProperty = DependencyProperty.Register(
            nameof(SkinId),
            typeof(int),
            typeof(HeroSkinView),
            new PropertyMetadata(1));

        /// <summary>Identifies the <see cref="SelectCommand"/> dependency property.</summary>
        public static readonly DependencyProperty SelectCommandProperty = DependencyProperty.Register(
            nameof(SelectCommand),
            typeof(ICommand),
            typeof(HeroSkinView),
            new PropertyMetadata(null));

        /// <summary>Initializes a new instance of the <see cref="HeroSkinView"/> class.</summary>
        public HeroSkinView()
        {
            InitializeComponent();
        }

        /// <summary>Gets or sets the current skin id shown on the button.</summary>
        public int SkinId
        {
            get => (int)GetValue(SkinIdProperty);
            set => SetValue(SkinIdProperty, value);
        }

        /// <summary>Gets or sets the command that changes the skin.</summary>
        public ICommand? SelectCommand
        {
            get => (ICommand?)GetValue(SelectCommandProperty);
            set => SetValue(SelectCommandProperty, value);
        }
    }
}
