using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Screen chrome reused by lobby screens: title on the left, close cross on the right.</summary>
    public partial class ScreenHeaderView : UserControl
    {
        /// <summary>Identifies the <see cref="Title"/> dependency property.</summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(ScreenHeaderView),
                new PropertyMetadata(string.Empty));

        /// <summary>Identifies the <see cref="CloseCommand"/> dependency property.</summary>
        public static readonly DependencyProperty CloseCommandProperty =
            DependencyProperty.Register(
                nameof(CloseCommand),
                typeof(ICommand),
                typeof(ScreenHeaderView),
                new PropertyMetadata(null));

        /// <summary>Gets or sets the screen title.</summary>
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>Gets or sets the command invoked by the close cross.</summary>
        public ICommand? CloseCommand
        {
            get { return (ICommand?)GetValue(CloseCommandProperty); }
            set { SetValue(CloseCommandProperty, value); }
        }

        /// <summary>Initializes a new instance of the <see cref="ScreenHeaderView"/> class.</summary>
        public ScreenHeaderView()
        {
            InitializeComponent();
        }
    }
}