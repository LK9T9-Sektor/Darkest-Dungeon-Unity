using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Sektor.DarkestDungeon.Wpf.Ui;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Shared battle-unit card: position, attack/target state, Spine or fallback art, HP and stress.</summary>
    public partial class BattleUnitCardView : UserControl
    {
        private const int HpSegmentCount = 12;
        private const int StressSegmentCount = 10;
        private const string DefaultActivityLabel = "АТАКА";

        /// <summary>Identifies the <see cref="Rank"/> dependency property.</summary>
        public static readonly DependencyProperty RankProperty = DependencyProperty.Register(
            nameof(Rank),
            typeof(int),
            typeof(BattleUnitCardView),
            new PropertyMetadata(0));

        /// <summary>Identifies the <see cref="Hp"/> dependency property.</summary>
        public static readonly DependencyProperty HpProperty = DependencyProperty.Register(
            nameof(Hp),
            typeof(int),
            typeof(BattleUnitCardView),
            new PropertyMetadata(0, OnBarValueChanged));

        /// <summary>Identifies the <see cref="MaxHp"/> dependency property.</summary>
        public static readonly DependencyProperty MaxHpProperty = DependencyProperty.Register(
            nameof(MaxHp),
            typeof(int),
            typeof(BattleUnitCardView),
            new PropertyMetadata(0, OnBarValueChanged));

        /// <summary>Identifies the <see cref="Stress"/> dependency property.</summary>
        public static readonly DependencyProperty StressProperty = DependencyProperty.Register(
            nameof(Stress),
            typeof(int),
            typeof(BattleUnitCardView),
            new PropertyMetadata(0, OnBarValueChanged));

        /// <summary>Identifies the <see cref="MaxStress"/> dependency property.</summary>
        public static readonly DependencyProperty MaxStressProperty = DependencyProperty.Register(
            nameof(MaxStress),
            typeof(int),
            typeof(BattleUnitCardView),
            new PropertyMetadata(0, OnBarValueChanged));

        /// <summary>Identifies the <see cref="ContentDirectory"/> dependency property.</summary>
        public static readonly DependencyProperty ContentDirectoryProperty = DependencyProperty.Register(
            nameof(ContentDirectory),
            typeof(string),
            typeof(BattleUnitCardView),
            new PropertyMetadata(string.Empty, OnContentDirectoryChanged));

        /// <summary>Identifies the read-only <see cref="HasSpineContent"/> dependency property.</summary>
        private static readonly DependencyPropertyKey HasSpineContentPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(HasSpineContent),
                typeof(bool),
                typeof(BattleUnitCardView),
                new PropertyMetadata(false));

        /// <summary>Identifies the read-only <see cref="HasSpineContent"/> dependency property.</summary>
        public static readonly DependencyProperty HasSpineContentProperty = HasSpineContentPropertyKey.DependencyProperty;

        /// <summary>Identifies the <see cref="Loop"/> dependency property.</summary>
        public static readonly DependencyProperty LoopProperty = DependencyProperty.Register(
            nameof(Loop),
            typeof(bool),
            typeof(BattleUnitCardView),
            new PropertyMetadata(true));

        /// <summary>Identifies the <see cref="Skeleton"/> dependency property.</summary>
        public static readonly DependencyProperty SkeletonProperty = DependencyProperty.Register(
            nameof(Skeleton),
            typeof(string),
            typeof(BattleUnitCardView),
            new PropertyMetadata(string.Empty));

        /// <summary>Identifies the <see cref="IsEnemy"/> dependency property.</summary>
        public static readonly DependencyProperty IsEnemyProperty = DependencyProperty.Register(
            nameof(IsEnemy),
            typeof(bool),
            typeof(BattleUnitCardView),
            new PropertyMetadata(false));

        /// <summary>Identifies the <see cref="IsAttacker"/> dependency property.</summary>
        public static readonly DependencyProperty IsAttackerProperty = DependencyProperty.Register(
            nameof(IsAttacker),
            typeof(bool),
            typeof(BattleUnitCardView),
            new PropertyMetadata(false));

        /// <summary>Identifies the <see cref="IsTarget"/> dependency property.</summary>
        public static readonly DependencyProperty IsTargetProperty = DependencyProperty.Register(
            nameof(IsTarget),
            typeof(bool),
            typeof(BattleUnitCardView),
            new PropertyMetadata(false));

        /// <summary>Identifies the <see cref="IsDead"/> dependency property.</summary>
        public static readonly DependencyProperty IsDeadProperty = DependencyProperty.Register(
            nameof(IsDead),
            typeof(bool),
            typeof(BattleUnitCardView),
            new PropertyMetadata(false));

        /// <summary>Identifies the <see cref="IsMirrored"/> dependency property.</summary>
        public static readonly DependencyProperty IsMirroredProperty = DependencyProperty.Register(
            nameof(IsMirrored),
            typeof(bool),
            typeof(BattleUnitCardView),
            new PropertyMetadata(false));

        /// <summary>Identifies the <see cref="InfoCommand"/> dependency property.</summary>
        public static readonly DependencyProperty InfoCommandProperty = DependencyProperty.Register(
            nameof(InfoCommand),
            typeof(ICommand),
            typeof(BattleUnitCardView),
            new PropertyMetadata(null));

        /// <summary>Identifies the <see cref="ActivityLabel"/> dependency property.</summary>
        public static readonly DependencyProperty ActivityLabelProperty = DependencyProperty.Register(
            nameof(ActivityLabel),
            typeof(string),
            typeof(BattleUnitCardView),
            new PropertyMetadata(string.Empty, OnActivityLabelChanged));

        /// <summary>Identifies the <see cref="ActivityTone"/> dependency property.</summary>
        public static readonly DependencyProperty ActivityToneProperty = DependencyProperty.Register(
            nameof(ActivityTone),
            typeof(SkillTone),
            typeof(BattleUnitCardView),
            new PropertyMetadata(SkillTone.Attack, OnActivityToneChanged));

        /// <summary>Identifies the read-only <see cref="ActivityBrush"/> dependency property.</summary>
        private static readonly DependencyPropertyKey ActivityBrushPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ActivityBrush),
                typeof(Brush),
                typeof(BattleUnitCardView),
                new PropertyMetadata(SkillToneClassifier.ArrowBrush(SkillTone.Attack)));

        /// <summary>Identifies the read-only <see cref="ActivityBrush"/> dependency property.</summary>
        public static readonly DependencyProperty ActivityBrushProperty = ActivityBrushPropertyKey.DependencyProperty;

        /// <summary>Identifies the read-only <see cref="ActivityText"/> dependency property.</summary>
        private static readonly DependencyPropertyKey ActivityTextPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ActivityText),
                typeof(string),
                typeof(BattleUnitCardView),
                new PropertyMetadata(DefaultActivityLabel));

        /// <summary>Identifies the read-only <see cref="ActivityText"/> dependency property.</summary>
        public static readonly DependencyProperty ActivityTextProperty = ActivityTextPropertyKey.DependencyProperty;

        /// <summary>Identifies the <see cref="AnimationCompletedCommand"/> dependency property.</summary>
        public static readonly DependencyProperty AnimationCompletedCommandProperty = DependencyProperty.Register(
            nameof(AnimationCompletedCommand),
            typeof(ICommand),
            typeof(BattleUnitCardView),
            new PropertyMetadata(null));

        /// <summary>Initializes a new instance of the <see cref="BattleUnitCardView"/> class.</summary>
        public BattleUnitCardView()
        {
            InitializeComponent();
            RebuildBars();
        }

        /// <summary>Gets the filled HP segments.</summary>
        public ObservableCollection<int> HpSegments { get; } = new ObservableCollection<int>();

        /// <summary>Gets the filled stress segments.</summary>
        public ObservableCollection<int> StressSegments { get; } = new ObservableCollection<int>();

        /// <summary>Gets or sets the formation position.</summary>
        public int Rank
        {
            get { return (int)GetValue(RankProperty); }
            set { SetValue(RankProperty, value); }
        }

        /// <summary>Gets or sets the current hit points.</summary>
        public int Hp
        {
            get { return (int)GetValue(HpProperty); }
            set { SetValue(HpProperty, value); }
        }

        /// <summary>Gets or sets the maximum hit points.</summary>
        public int MaxHp
        {
            get { return (int)GetValue(MaxHpProperty); }
            set { SetValue(MaxHpProperty, value); }
        }

        /// <summary>Gets or sets the current stress.</summary>
        public int Stress
        {
            get { return (int)GetValue(StressProperty); }
            set { SetValue(StressProperty, value); }
        }

        /// <summary>Gets or sets the maximum stress.</summary>
        public int MaxStress
        {
            get { return (int)GetValue(MaxStressProperty); }
            set { SetValue(MaxStressProperty, value); }
        }

        /// <summary>Gets or sets the Spine content directory.</summary>
        public string ContentDirectory
        {
            get { return (string)GetValue(ContentDirectoryProperty); }
            set { SetValue(ContentDirectoryProperty, value); }
        }

        /// <summary>Gets a value indicating whether the Spine content is available.</summary>
        public bool HasSpineContent
        {
            get { return (bool)GetValue(HasSpineContentProperty); }
        }

        /// <summary>Gets or sets a value indicating whether the Spine animation loops.</summary>
        public bool Loop
        {
            get { return (bool)GetValue(LoopProperty); }
            set { SetValue(LoopProperty, value); }
        }

        /// <summary>Gets or sets the skeleton id shown by the fallback card.</summary>
        public string Skeleton
        {
            get { return (string)GetValue(SkeletonProperty); }
            set { SetValue(SkeletonProperty, value); }
        }

        /// <summary>Gets or sets a value indicating whether the unit belongs to the enemy side.</summary>
        public bool IsEnemy
        {
            get { return (bool)GetValue(IsEnemyProperty); }
            set { SetValue(IsEnemyProperty, value); }
        }

        /// <summary>Gets or sets a value indicating whether the unit is the current attacker.</summary>
        public bool IsAttacker
        {
            get { return (bool)GetValue(IsAttackerProperty); }
            set { SetValue(IsAttackerProperty, value); }
        }

        /// <summary>Gets or sets a value indicating whether the unit is the current target.</summary>
        public bool IsTarget
        {
            get { return (bool)GetValue(IsTargetProperty); }
            set { SetValue(IsTargetProperty, value); }
        }

        /// <summary>Gets or sets a value indicating whether the unit is dead.</summary>
        public bool IsDead
        {
            get { return (bool)GetValue(IsDeadProperty); }
            set { SetValue(IsDeadProperty, value); }
        }

        /// <summary>Gets or sets a value indicating whether the Spine art is mirrored horizontally.</summary>
        public bool IsMirrored
        {
            get { return (bool)GetValue(IsMirroredProperty); }
            set { SetValue(IsMirroredProperty, value); }
        }

        /// <summary>Gets or sets the command opening the buff/debuff table.</summary>
        public ICommand? InfoCommand
        {
            get { return (ICommand?)GetValue(InfoCommandProperty); }
            set { SetValue(InfoCommandProperty, value); }
        }

        /// <summary>Gets or sets the command invoked when the Spine animation completes.</summary>
        public ICommand? AnimationCompletedCommand
        {
            get { return (ICommand?)GetValue(AnimationCompletedCommandProperty); }
            set { SetValue(AnimationCompletedCommandProperty, value); }
        }

        /// <summary>Gets or sets the activity badge text; an empty value keeps the default label.</summary>
        public string ActivityLabel
        {
            get { return (string)GetValue(ActivityLabelProperty); }
            set { SetValue(ActivityLabelProperty, value); }
        }

        /// <summary>Gets the badge text actually rendered (the default label when no activity is set).</summary>
        public string ActivityText
        {
            get { return (string)GetValue(ActivityTextProperty); }
        }

        /// <summary>Gets or sets the tone that colors the activity badges (attack/heal/buff).</summary>
        public SkillTone ActivityTone
        {
            get { return (SkillTone)GetValue(ActivityToneProperty); }
            set { SetValue(ActivityToneProperty, value); }
        }

        /// <summary>Gets the badge fill brush derived from the activity tone.</summary>
        public Brush ActivityBrush
        {
            get { return (Brush)GetValue(ActivityBrushProperty); }
        }

        private void OnInfoRightClick(object sender, MouseButtonEventArgs e)
        {
            if (InfoCommand == null)
                return;

            object? parameter = (sender as Button)?.CommandParameter;
            if (InfoCommand.CanExecute(parameter))
            {
                InfoCommand.Execute(parameter);
                e.Handled = true;
            }
        }

        private static void OnActivityLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (BattleUnitCardView)d;
            string label = e.NewValue as string ?? string.Empty;
            control.SetValue(ActivityTextPropertyKey, label.Length == 0 ? DefaultActivityLabel : label);
        }

        private static void OnActivityToneChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (BattleUnitCardView)d;
            control.SetValue(ActivityBrushPropertyKey, SkillToneClassifier.ArrowBrush((SkillTone)e.NewValue));
        }

        private static void OnBarValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BattleUnitCardView)d).RebuildBars();
        }

        private static void OnContentDirectoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (BattleUnitCardView)d;
            control.SetValue(HasSpineContentPropertyKey, !string.IsNullOrEmpty(e.NewValue as string));
        }

        private void RebuildBars()
        {
            HpSegments.Clear();
            double hpRatio = MaxHp <= 0 ? 0 : Math.Max(0, Math.Min(1, (double)Hp / MaxHp));
            int hpFilled = (int)Math.Round(hpRatio * HpSegmentCount);
            for (int i = 0; i < HpSegmentCount; i++)
                HpSegments.Add(i < hpFilled ? 1 : 0);

            StressSegments.Clear();
            double stressRatio = MaxStress <= 0 ? 0 : Math.Max(0, Math.Min(1, (double)Stress / MaxStress));
            int stressFilled = (int)Math.Round(stressRatio * StressSegmentCount);
            for (int i = 0; i < StressSegmentCount; i++)
                StressSegments.Add(i < stressFilled ? 1 : 0);
        }
    }
}
