using System.ComponentModel;
using System.Windows;
using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Duel battle screen; pumps the rival link while it is visible.</summary>
    public partial class DuelBattleView : PumpableScreenBase
    {
        /// <summary>Initializes a new instance of the <see cref="DuelBattleView"/> class.</summary>
        public DuelBattleView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is INotifyPropertyChanged oldVm)
                oldVm.PropertyChanged -= OnViewModelPropertyChanged;
            if (e.NewValue is INotifyPropertyChanged newVm)
                newVm.PropertyChanged += OnViewModelPropertyChanged;
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DuelBattleViewModel.AiSkillPreview)
                || e.PropertyName == nameof(DuelBattleViewModel.SelectedSkill))
            {
                UpdateBadge();
            }
        }
    }
}