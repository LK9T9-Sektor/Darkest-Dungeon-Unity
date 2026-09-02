using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Duel battle screen; pumps the rival link while it is visible.</summary>
    public partial class DuelBattleView : PumpableScreenBase
    {
        /// <summary>Gets the fixed skill arrow slots (one elbow arrow per target, up to four).</summary>
        private readonly Path[] SkillArrowPaths;

        /// <summary>Gets the fixed skill arrowhead slots matching <see cref="SkillArrowPaths"/>.</summary>
        private readonly Polygon[] SkillArrowHeads;

        /// <summary>Initializes a new instance of the <see cref="DuelBattleView"/> class.</summary>
        public DuelBattleView()
        {
            InitializeComponent();
            SkillArrowPaths = new[] { SkillArrow1, SkillArrow2, SkillArrow3, SkillArrow4 };
            SkillArrowHeads = new[] { SkillArrowHead1, SkillArrowHead2, SkillArrowHead3, SkillArrowHead4 };
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
            switch (e.PropertyName)
            {
                case nameof(DuelBattleViewModel.AiSkillPreview):
                case nameof(DuelBattleViewModel.SelectedSkill):
                    UpdateBadge();
                    break;
                case nameof(DuelBattleViewModel.AiTargetPreview):
                case nameof(DuelBattleViewModel.IsMovePreview):
                    if (DataContext is DuelBattleViewModel viewModel)
                        RedrawAiArrow(viewModel);
                    break;
            }
        }
    }
}