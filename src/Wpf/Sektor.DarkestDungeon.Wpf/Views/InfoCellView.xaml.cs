using System.Windows;
using System.Windows.Controls;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Compact caption/value cell used by the battle scene bar.</summary>
    public partial class InfoCellView : UserControl
    {
        /// <summary>Identifies the <see cref="Label"/> dependency property.</summary>
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            nameof(Label),
            typeof(string),
            typeof(InfoCellView),
            new PropertyMetadata(string.Empty));

        /// <summary>Identifies the <see cref="Value"/> dependency property.</summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(string),
            typeof(InfoCellView),
            new PropertyMetadata(string.Empty));

        /// <summary>Identifies the <see cref="Detail"/> dependency property.</summary>
        public static readonly DependencyProperty DetailProperty = DependencyProperty.Register(
            nameof(Detail),
            typeof(string),
            typeof(InfoCellView),
            new PropertyMetadata(string.Empty));

        /// <summary>Initializes a new instance of the <see cref="InfoCellView"/> class.</summary>
        public InfoCellView()
        {
            InitializeComponent();
        }

        /// <summary>Gets or sets the caption shown before the value.</summary>
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        /// <summary>Gets or sets the main value.</summary>
        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>Gets or sets the optional muted detail shown after the value.</summary>
        public string Detail
        {
            get { return (string)GetValue(DetailProperty); }
            set { SetValue(DetailProperty, value); }
        }
    }
}
