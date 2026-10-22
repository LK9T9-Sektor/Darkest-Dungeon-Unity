using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sektor.DarkestDungeon.Core.Common;
using Sektor.DarkestDungeon.Wpf.Data;
using Sektor.DarkestDungeon.Wpf.Navigation;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>
    /// Visual 1x1 arena: a hero on the left plays its attack once (the zero-duration attack pose
    /// stub underlay + the timed skill-effect skeleton overlay, mirroring Unity's layering); when
    /// the overlay finishes, the target on the right flashes damage and shows a popup, and both
    /// sides return to their idle sets. Purely presentational (no duel core behind it).
    /// </summary>
    public partial class SpineBattleViewModel : ObservableObject
    {
        /// <summary>The hero occupying the attacker slot.</summary>
        public const string AttackerHero = "Crusader";

        /// <summary>The hero occupying the target slot.</summary>
        public const string TargetHero = "Highwayman";

        /// <summary>The idle animation set of both slots.</summary>
        public const string IdleAnimation = "Idle";

        /// <summary>The attack pose set of the attacker (zero-duration stub, underlay layer).</summary>
        public const string AttackPoseAnimation = "AttackSword";

        /// <summary>The timed skill-effect set played over the pose (drives completion, Unity parity: Abilities layer).</summary>
        public const string AttackSkillAnimation = "Smite";

        /// <summary>The card flash value bound by the damage flash trigger in the view.</summary>
        public const string DamageFlash = "Damage";

        /// <summary>The damage number shown by the target popup.</summary>
        public const string DamagePopup = "-6";

        private readonly INavigationService navigation;
        private readonly ISpineDemoCatalog catalog;
        private readonly ILogger logger;
        private readonly Func<object> createCaptureReplay;

        /// <summary>Gets or sets the content directory of the attacker slot.</summary>
        [ObservableProperty]
        private string _attackerContentDirectory = string.Empty;

        /// <summary>Gets or sets a value indicating whether the attacker animation loops (false while attacking).</summary>
        [ObservableProperty]
        private bool _attackerLoop = true;

        /// <summary>
        /// Gets or sets the content directory of the attacker's skill-effect overlay; empty between
        /// attacks (the control treats an empty directory as switched off).
        /// </summary>
        [ObservableProperty]
        private string _attackerOverlayContentDirectory = string.Empty;

        /// <summary>Gets or sets the content directory of the target slot.</summary>
        [ObservableProperty]
        private string _targetContentDirectory = string.Empty;

        /// <summary>Gets or sets the target flash value (see <see cref="DamageFlash"/>); empty when idle.</summary>
        [ObservableProperty]
        private string _targetCardFlash = string.Empty;

        /// <summary>Gets or sets the target damage popup text.</summary>
        [ObservableProperty]
        private string _targetPopupText = string.Empty;

        /// <summary>Gets or sets a value indicating whether the target damage popup is visible.</summary>
        [ObservableProperty]
        private bool _targetPopupVisible;

        /// <summary>Gets or sets a value indicating whether an attack is currently playing.</summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AttackCommand))]
        private bool _isAttacking;

        /// <summary>Gets the screen title.</summary>
        public string ScreenTitle => "РЕПЛЕЙ";

        /// <summary>Gets the attacker slot name.</summary>
        public string AttackerName => AttackerHero.ToUpperInvariant();

        /// <summary>Gets the target slot name.</summary>
        public string TargetName => TargetHero.ToUpperInvariant();

        /// <summary>Gets the command returning to the main menu.</summary>
        public IRelayCommand BackCommand { get; }

        /// <summary>Gets the command starting the one-shot attack animation.</summary>
        public IRelayCommand AttackCommand { get; }

        /// <summary>Gets the command opening the recorded-capture replay screen.</summary>
        public IRelayCommand CaptureReplayCommand { get; }

        /// <summary>
        /// Gets the command executed by the attacker control when its non-looping animation
        /// finished; the parameter is the completed animation name.
        /// </summary>
        public IRelayCommand<string?> AttackerAnimationCompletedCommand { get; }

        /// <summary>Initializes a new instance of the <see cref="SpineBattleViewModel"/> class.</summary>
        /// <param name="navigation">The navigation service.</param>
        /// <param name="catalog">The catalog resolving animation set content directories.</param>
        /// <param name="logger">The structural logger.</param>
        /// <param name="createCaptureReplay">Creates the recorded-capture replay screen.</param>
        public SpineBattleViewModel(
            INavigationService navigation,
            ISpineDemoCatalog catalog,
            ILogger logger,
            Func<object> createCaptureReplay)
        {
            this.navigation = navigation;
            this.catalog = catalog;
            this.logger = logger;
            this.createCaptureReplay = createCaptureReplay ?? throw new ArgumentNullException(nameof(createCaptureReplay));

            AttackerContentDirectory = catalog.GetContentDirectory(AttackerHero, IdleAnimation);
            TargetContentDirectory = catalog.GetContentDirectory(TargetHero, IdleAnimation);

            BackCommand = new RelayCommand(navigation.GoHome);
            AttackCommand = new RelayCommand(StartAttack, () => !IsAttacking);
            CaptureReplayCommand = new RelayCommand(OpenCaptureReplay);
            AttackerAnimationCompletedCommand = new RelayCommand<string?>(OnAttackerAnimationCompleted);

            logger.Log($"arena: opened attacker={AttackerHero} target={TargetHero}");
        }

        private void StartAttack()
        {
            if (IsAttacking)
                return;

            IsAttacking = true;
            AttackerContentDirectory = catalog.GetContentDirectory(AttackerHero, AttackPoseAnimation);
            AttackerOverlayContentDirectory = catalog.GetContentDirectory(AttackerHero, AttackSkillAnimation);
            logger.Log(
                $"arena: attack started attacker={AttackerHero} pose={AttackPoseAnimation} skill={AttackSkillAnimation}");
        }

        private void OpenCaptureReplay()
        {
            logger.Log("arena: opening capture replay");
            navigation.NavigateTo(createCaptureReplay());
        }

        private void OnAttackerAnimationCompleted(string? animationName)
        {
            if (!IsAttacking)
                return;

            AttackerOverlayContentDirectory = string.Empty;
            AttackerContentDirectory = catalog.GetContentDirectory(AttackerHero, IdleAnimation);

            TargetPopupVisible = false;
            TargetPopupText = DamagePopup;
            TargetPopupVisible = true;
            TargetCardFlash = string.Empty;
            TargetCardFlash = DamageFlash;

            IsAttacking = false;
            logger.Log(
                $"arena: attack completed animation={animationName} target={TargetHero} damage={DamagePopup}");
        }
    }
}
