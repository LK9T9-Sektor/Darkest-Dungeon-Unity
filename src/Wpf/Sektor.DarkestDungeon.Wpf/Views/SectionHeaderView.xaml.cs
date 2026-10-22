using System.Windows;
using System.Windows.Controls;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Reusable section heading: a centered caption with a bottom separator. The host supplies
    /// only <see cref="Text"/>, so section headers stay visually identical across screens.</summary>
    public partial class SectionHeaderView : UserControl
    {
        /// <summary>Identifies the <see cref="Text"/> dependency property.</summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(SectionHeaderView),
            new PropertyMetadata(string.Empty));

        /// <summary>Initializes a new instance of the <see cref="SectionHeaderView"/> class.</summary>
        public SectionHeaderView()
        {
            InitializeComponent();
        }

        /// <summary>Gets or sets the heading text.</summary>
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
    }
}
