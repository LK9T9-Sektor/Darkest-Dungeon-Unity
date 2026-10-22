using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Reusable lobby table: icon, class/unit name and occupied slots. Rows with more
    /// than one occupied slot are shown but cannot be selected.</summary>
    public partial class UnitSelectionTableView : UserControl
    {
        /// <summary>Identifies the <see cref="Rows"/> dependency property.</summary>
        public static readonly DependencyProperty RowsProperty = DependencyProperty.Register(
            nameof(Rows),
            typeof(IEnumerable),
            typeof(UnitSelectionTableView),
            new PropertyMetadata(null));

        /// <summary>Identifies the <see cref="ActivateCommand"/> dependency property.</summary>
        public static readonly DependencyProperty ActivateCommandProperty = DependencyProperty.Register(
            nameof(ActivateCommand),
            typeof(ICommand),
            typeof(UnitSelectionTableView),
            new PropertyMetadata(null));

        /// <summary>Initializes a new instance of the <see cref="UnitSelectionTableView"/> class.</summary>
        public UnitSelectionTableView()
        {
            InitializeComponent();
        }

        /// <summary>Gets or sets the selectable unit rows.</summary>
        public IEnumerable Rows
        {
            get { return (IEnumerable)GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        /// <summary>Gets or sets the command invoked when a row is double-clicked.</summary>
        public ICommand ActivateCommand
        {
            get { return (ICommand)GetValue(ActivateCommandProperty); }
            set { SetValue(ActivateCommandProperty, value); }
        }

        private void OnRowMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2 ||
                !(sender is FrameworkElement element) ||
                !(element.DataContext is UnitSelectionRowViewModel row))
            {
                return;
            }

            if (ActivateCommand != null && ActivateCommand.CanExecute(row))
                ActivateCommand.Execute(row);

            e.Handled = true;
        }
    }
}
