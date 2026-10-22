using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Reusable hero identity block: edit/reroll buttons, hero name and class.</summary>
    public partial class HeroIdentityView : UserControl
    {
        /// <summary>Identifies the <see cref="HeroName"/> dependency property.</summary>
        public static readonly DependencyProperty HeroNameProperty = DependencyProperty.Register(
            nameof(HeroName),
            typeof(string),
            typeof(HeroIdentityView),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>Identifies the <see cref="ClassName"/> dependency property.</summary>
        public static readonly DependencyProperty ClassNameProperty = DependencyProperty.Register(
            nameof(ClassName),
            typeof(string),
            typeof(HeroIdentityView),
            new PropertyMetadata(string.Empty));

        /// <summary>Identifies the <see cref="IsEditing"/> dependency property.</summary>
        public static readonly DependencyProperty IsEditingProperty = DependencyProperty.Register(
            nameof(IsEditing),
            typeof(bool),
            typeof(HeroIdentityView),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsEditingChanged));

        /// <summary>Identifies the <see cref="MaxNameLength"/> dependency property.</summary>
        public static readonly DependencyProperty MaxNameLengthProperty = DependencyProperty.Register(
            nameof(MaxNameLength),
            typeof(int),
            typeof(HeroIdentityView),
            new PropertyMetadata(24, OnMaxNameLengthChanged));

        /// <summary>Identifies the <see cref="EditCommand"/> dependency property.</summary>
        public static readonly DependencyProperty EditCommandProperty = DependencyProperty.Register(
            nameof(EditCommand),
            typeof(ICommand),
            typeof(HeroIdentityView),
            new PropertyMetadata(null));

        /// <summary>Identifies the <see cref="RerollCommand"/> dependency property.</summary>
        public static readonly DependencyProperty RerollCommandProperty = DependencyProperty.Register(
            nameof(RerollCommand),
            typeof(ICommand),
            typeof(HeroIdentityView),
            new PropertyMetadata(null));

        private string editStartName = string.Empty;

        /// <summary>Initializes a new instance of the <see cref="HeroIdentityView"/> class.</summary>
        public HeroIdentityView()
        {
            InitializeComponent();
            UpdateNameHint();
        }

        /// <summary>Gets or sets the hero name.</summary>
        public string HeroName
        {
            get => (string)GetValue(HeroNameProperty);
            set => SetValue(HeroNameProperty, value);
        }

        /// <summary>Gets or sets the hero class display name.</summary>
        public string ClassName
        {
            get => (string)GetValue(ClassNameProperty);
            set => SetValue(ClassNameProperty, value);
        }

        /// <summary>Gets or sets a value indicating whether the name is being edited.</summary>
        public bool IsEditing
        {
            get => (bool)GetValue(IsEditingProperty);
            set => SetValue(IsEditingProperty, value);
        }

        /// <summary>Gets or sets the maximum number of characters accepted for the name.</summary>
        public int MaxNameLength
        {
            get => (int)GetValue(MaxNameLengthProperty);
            set => SetValue(MaxNameLengthProperty, value);
        }

        /// <summary>Gets or sets the command that enters name editing.</summary>
        public ICommand? EditCommand
        {
            get => (ICommand?)GetValue(EditCommandProperty);
            set => SetValue(EditCommandProperty, value);
        }

        /// <summary>Gets or sets the command that rerolls the name.</summary>
        public ICommand? RerollCommand
        {
            get => (ICommand?)GetValue(RerollCommandProperty);
            set => SetValue(RerollCommandProperty, value);
        }

        private static void OnIsEditingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HeroIdentityView)d).UpdateEditingState();
        }

        private static void OnMaxNameLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HeroIdentityView)d).UpdateNameHint();
        }

        private void UpdateNameHint()
        {
            if (NameHint != null)
                NameHint.Text = new string('.', System.Math.Max(0, MaxNameLength));
        }

        private void UpdateEditingState()
        {
            bool editing = IsEditing;
            if (editing && !EditPanel.IsVisible)
                editStartName = HeroName;

            DisplayPanel.Visibility = editing ? Visibility.Collapsed : Visibility.Visible;
            EditPanel.Visibility = editing ? Visibility.Visible : Visibility.Collapsed;

            if (editing)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new System.Action(() =>
                {
                    NameBox.Focus();
                    NameBox.SelectAll();
                }));
            }
        }

        private void OnNameKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                IsEditing = false;
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                HeroName = editStartName;
                IsEditing = false;
                e.Handled = true;
            }
        }

        private void OnNameLostFocus(object sender, RoutedEventArgs e)
        {
            if (IsEditing)
                IsEditing = false;
        }
    }
}
