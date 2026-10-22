using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Centered buff/debuff table for a single battle unit.</summary>
    public partial class BuffTableOverlayView : UserControl
    {
        /// <summary>Identifies the <see cref="Target"/> dependency property.</summary>
        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(
            nameof(Target),
            typeof(DuelUnitViewModel),
            typeof(BuffTableOverlayView),
            new PropertyMetadata(null));

        /// <summary>Identifies the <see cref="CloseCommand"/> dependency property.</summary>
        public static readonly DependencyProperty CloseCommandProperty = DependencyProperty.Register(
            nameof(CloseCommand),
            typeof(ICommand),
            typeof(BuffTableOverlayView),
            new PropertyMetadata(null));

        /// <summary>Initializes a new instance of the <see cref="BuffTableOverlayView"/> class.</summary>
        public BuffTableOverlayView()
        {
            InitializeComponent();
        }

        /// <summary>Gets or sets the unit whose buffs and debuffs are shown.</summary>
        public DuelUnitViewModel? Target
        {
            get { return (DuelUnitViewModel?)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }

        /// <summary>Gets or sets the command closing the table.</summary>
        public ICommand? CloseCommand
        {
            get { return (ICommand?)GetValue(CloseCommandProperty); }
            set { SetValue(CloseCommandProperty, value); }
        }
    }
}
