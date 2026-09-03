using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Reusable unit header: rank badge, colored name, optional class and close button.</summary>
    public partial class UnitHeaderView : UserControl
    {
        /// <summary>Identifies the <see cref="Rank"/> dependency property.</summary>
        public static readonly DependencyProperty RankProperty = DependencyProperty.Register(
            nameof(Rank), typeof(int), typeof(UnitHeaderView), new PropertyMetadata(0));

        /// <summary>Identifies the <see cref="DisplayName"/> dependency property.</summary>
        public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.Register(
            nameof(DisplayName), typeof(string), typeof(UnitHeaderView), new PropertyMetadata(string.Empty));

        /// <summary>Identifies the <see cref="ClassName"/> dependency property.</summary>
        public static readonly DependencyProperty ClassNameProperty = DependencyProperty.Register(
            nameof(ClassName), typeof(string), typeof(UnitHeaderView), new PropertyMetadata(string.Empty));

        /// <summary>Identifies the <see cref="IsEnemy"/> dependency property.</summary>
        public static readonly DependencyProperty IsEnemyProperty = DependencyProperty.Register(
            nameof(IsEnemy), typeof(bool), typeof(UnitHeaderView), new PropertyMetadata(false));

        /// <summary>Identifies the <see cref="CloseCommand"/> dependency property.</summary>
        public static readonly DependencyProperty CloseCommandProperty = DependencyProperty.Register(
            nameof(CloseCommand), typeof(ICommand), typeof(UnitHeaderView),
            new PropertyMetadata(null, OnCloseCommandChanged));

        /// <summary>Initializes a new instance of the <see cref="UnitHeaderView"/> class.</summary>
        public UnitHeaderView()
        {
            InitializeComponent();
            CloseButton.Visibility = CloseCommand == null ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>Gets or sets the formation rank.</summary>
        public int Rank
        {
            get { return (int)GetValue(RankProperty); }
            set { SetValue(RankProperty, value); }
        }

        /// <summary>Gets or sets the display name.</summary>
        public string DisplayName
        {
            get { return (string)GetValue(DisplayNameProperty); }
            set { SetValue(DisplayNameProperty, value); }
        }

        /// <summary>Gets or sets the class label (hidden when empty).</summary>
        public string ClassName
        {
            get { return (string)GetValue(ClassNameProperty); }
            set { SetValue(ClassNameProperty, value); }
        }

        /// <summary>Gets or sets a value indicating whether the unit belongs to the enemy side.</summary>
        public bool IsEnemy
        {
            get { return (bool)GetValue(IsEnemyProperty); }
            set { SetValue(IsEnemyProperty, value); }
        }

        /// <summary>Gets or sets the close button command (null hides the button).</summary>
        public ICommand CloseCommand
        {
            get { return (ICommand)GetValue(CloseCommandProperty); }
            set { SetValue(CloseCommandProperty, value); }
        }

        private static void OnCloseCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (UnitHeaderView)d;
            control.CloseButton.Visibility = e.NewValue == null ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}