using System.Windows;
using System.Windows.Controls;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Torch label, progress bar and value for the battle scene bar.</summary>
    public partial class TorchInfoView : UserControl
    {
        /// <summary>Identifies the <see cref="Value"/> dependency property.</summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(int),
            typeof(TorchInfoView),
            new PropertyMetadata(0));

        /// <summary>Initializes a new instance of the <see cref="TorchInfoView"/> class.</summary>
        public TorchInfoView()
        {
            InitializeComponent();
        }

        /// <summary>Gets or sets the current torch value.</summary>
        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
    }
}
