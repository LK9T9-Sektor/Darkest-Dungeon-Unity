using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Party;
using Sektor.DarkestDungeon.Core.Duel;
using Sektor.DarkestDungeon.Wpf.Data;
using Sektor.DarkestDungeon.Wpf.Networking;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>
    /// PvE battle driver: heroes of the player side against campaign monsters on the pure core
    /// (<see cref="DuelController.StartFight"/>). Mirrors the Unity <c>CoreBattleDriver</c> routing:
    /// the player controls the hero side (remote in <c>StartFight</c>, input via
    /// <see cref="DuelController.ApplyRemoteSkill"/>), monsters act locally through their campaign
    /// brains (<see cref="BattleSolver.UseMonsterBrain"/>). Reuses the duel battle view surface.
    /// </summary>
    public partial class PveBattleViewModel : ObservableObject, IPumpable, IDuelBattleViewData
    {
        private const double AiActionDelaySeconds = 0.4;
        private const double PumpTickSeconds = 0.05;
        private const string QuestText = "Defeat the enemy party";

        private readonly DuelController controller;
        private readonly Action onLeave;
        private readonly DispatcherTimer popupTimer;
        private readonly List<(int CombatId, string Text)> pendingPopups = new List<(int CombatId, string Text)>();
        private readonly Dictionary<int, Queue<string>> popupQueues = new Dictionary<int, Queue<string>>();

        private readonly FormationDisplayOrder heroOrder = FormationDisplayOrder.HeroSide();
        private readonly FormationDisplayOrder monsterOrder = FormationDisplayOrder.MonsterSide();

        private int _lastActorCombatId = -1;
        private int _lastRound = -1;
        private bool _turnInitialized;
        private bool _winnerAnnounced;
        private string _winnerText = string.Empty;
        private double _aiTimer;
        private string? selectedSkillId;
        private DuelSkillViewModel? selectedSkill;
        private bool isMoveMode;

        /// <summary>Gets the local party unit cards (left ranks).</summary>
        public ObservableCollection<DuelUnitViewModel> Heroes { get; } = new ObservableCollection<DuelUnitViewModel>();

        /// <summary>Gets the enemy party unit cards (right ranks).</summary>
        public ObservableCollection<DuelUnitViewModel> Monsters { get; } = new ObservableCollection<DuelUnitViewModel>();

        /// <summary>Gets the skill buttons of the acting unit.</summary>
        public ObservableCollection<DuelSkillViewModel> Skills { get; } = new ObservableCollection<DuelSkillViewModel>();

        /// <summary>Gets the current round's turn order strip.</summary>
        public ObservableCollection<DuelTurnEntryViewModel> TurnOrder { get; } = new ObservableCollection<DuelTurnEntryViewModel>();

        /// <summary>Gets the battle log lines.</summary>
        public ObservableCollection<string> Log { get; } = new ObservableCollection<string>();

        /// <summary>Gets the events overlay (round number, announcements).</summary>
        public EventsLayerViewModel Events { get; } = new EventsLayerViewModel();

        /// <summary>Gets the quest panel (title, goal).</summary>
        public QuestLogViewModel Quest { get; } = new QuestLogViewModel();

        /// <summary>Gets the bottom raid HUD (actor info, log/inventory/map).</summary>
        public RaidHudViewModel RaidHud { get; } = new RaidHudViewModel();

        /// <summary>Gets the top-center torch meter.</summary>
        public TorchViewModel Torch { get; } = new TorchViewModel();

        /// <summary>Gets the stat sheet shown when a unit is right-clicked.</summary>
        public HeroStatsViewModel StatsTarget { get; } = new HeroStatsViewModel();

        /// <summary>Gets or sets a value indicating whether the stats sheet overlay is visible.</summary>
        [ObservableProperty]
        private bool _isStatsVisible;

        /// <summary>Gets or sets the unit whose buff/debuff table is displayed.</summary>
        [ObservableProperty]
        private DuelUnitViewModel? _buffTarget;

        /// <summary>Gets or sets a value indicating whether the buff/debuff table overlay is visible.</summary>
        [ObservableProperty]
        private bool _isBuffTableVisible;

        /// <summary>Gets or sets the status line (round / turn / result).</summary>
        [ObservableProperty]
        private string _status = string.Empty;

        /// <summary>Gets the command that selects a skill.</summary>
        public IRelayCommand<DuelSkillViewModel> SelectSkillCommand { get; }

        /// <summary>Gets the command that targets and executes.</summary>
        public IRelayCommand<DuelUnitViewModel> TargetCommand { get; }

        /// <summary>Gets the command that starts a move of the acting unit to an adjacent rank.</summary>
        public IRelayCommand MoveCommand { get; }

        /// <summary>Gets the command that skips the acting unit's turn.</summary>
        public IRelayCommand PassCommand { get; }

        /// <summary>Gets the command that abandons the battle and returns to the main menu.</summary>
        public IRelayCommand LeaveCommand { get; }

        /// <summary>Gets the command that opens the stats sheet for the given unit.</summary>
        public IRelayCommand<DuelUnitViewModel> OpenStatsCommand { get; }

        /// <summary>Gets the command that closes the stats sheet.</summary>
        public IRelayCommand CloseStatsCommand { get; }

        /// <summary>Gets the command that toggles the buff/debuff table for the given unit.</summary>
        public IRelayCommand<DuelUnitViewModel> ToggleBuffTableCommand { get; }

        /// <summary>Gets the command that closes the buff/debuff table.</summary>
        public IRelayCommand CloseBuffTableCommand { get; }

        /// <summary>Gets the currently selected skill (badge source above the acting card), or null.</summary>
        public DuelSkillViewModel? SelectedSkill { get { return selectedSkill; } }

        /// <summary>Gets a value indicating whether it is the player's turn (a hero is acting).</summary>
        public bool IsLocalTurn { get { return controller.CurrentUnit?.Team == Team.Heroes; } }

        /// <summary>Gets a value indicating whether the move mode (adjacent rank swap) is active.</summary>
        public bool IsMoveMode { get { return isMoveMode; } }

        /// <summary>Gets a value indicating whether the AI preview is a move rather than a skill arrow.</summary>
        public bool IsMovePreview { get { return false; } }

        /// <summary>Gets the tone of the selected skill (attack/heal/buff) for the target arrow color.</summary>
        public Ui.SkillTone SelectedSkillTone
        {
            get
            {
                var unit = controller.CurrentUnit;
                var skill = unit?.Character.CurrentCombatSkills?.FirstOrDefault(candidate => candidate.Id == selectedSkillId);
                return Ui.SkillToneClassifier.Classify(skill);
            }
        }

        /// <summary>Gets a value indicating whether the selected skill targets multiple units at once (AOE / party).</summary>
        public bool SelectedSkillIsMultiTarget
        {
            get
            {
                var unit = controller.CurrentUnit;
                var skill = unit?.Character.CurrentCombatSkills?.FirstOrDefault(candidate => candidate.Id == selectedSkillId);
                return skill != null
                    && (skill.TargetRanks != null
                        && (skill.TargetRanks.IsSelfFormation || skill.TargetRanks.Ranks.Count > 1));
            }
        }

        /// <summary>Gets the AI (monster) skill preview shown in the badge, or null (monsters act immediately).</summary>
        public DuelSkillViewModel? AiSkillPreview { get { return null; } }

        /// <summary>Gets the AI (monster) target preview card, or null.</summary>
        public DuelUnitViewModel? AiTargetPreview { get { return null; } }

        /// <summary>Initializes a new instance of the <see cref="PveBattleViewModel"/> class.</summary>
        /// <param name="controller">The started duel controller (heroes vs monsters).</param>
        /// <param name="onLeave">Invoked when the player abandons the battle.</param>
        public PveBattleViewModel(DuelController controller, Action onLeave)
        {
            this.controller = controller;
            this.onLeave = onLeave;

            SelectSkillCommand = new RelayCommand<DuelSkillViewModel>(SelectSkill);
            TargetCommand = new RelayCommand<DuelUnitViewModel>(SelectTarget);
            MoveCommand = new RelayCommand(SelectMove);
            PassCommand = new RelayCommand(Pass);
            LeaveCommand = new RelayCommand(Leave);
            OpenStatsCommand = new RelayCommand<DuelUnitViewModel>(OpenStats);
            CloseStatsCommand = new RelayCommand(() => IsStatsVisible = false);
            ToggleBuffTableCommand = new RelayCommand<DuelUnitViewModel>(ToggleBuffTable);
            CloseBuffTableCommand = new RelayCommand(() => IsBuffTableVisible = false);

            controller.Events.StateChanged += Refresh;
            controller.Events.PopupShown += OnPopupShown;
            Quest.Title = "PvE Battle";
            Quest.Goal = QuestText;

            popupTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(2400) };
            popupTimer.Tick += (s, e) => AdvancePopups();
            popupTimer.Start();

            Refresh();
        }

        /// <summary>Gets the running core duel controller.</summary>
        public DuelController Duel { get { return controller; } }

        /// <summary>Gets a value indicating whether the battle has finished.</summary>
        public bool IsFinished { get { return controller.IsFinished; } }

        /// <inheritdoc/>
        public void Pump()
        {
            if (controller.IsFinished)
            {
                AnnounceWinner();
                return;
            }

            var actor = controller.CurrentUnit;
            if (actor == null)
                return;

            if (actor.CombatInfo.CombatId != _lastActorCombatId)
            {
                _lastActorCombatId = actor.CombatInfo.CombatId;
                _turnInitialized = false;
            }

            if (actor.Team == Team.Heroes)
            {
                if (!_turnInitialized)
                {
                    _turnInitialized = true;
                    Refresh();
                }

                return;
            }

            if (!_turnInitialized)
            {
                _turnInitialized = true;
                _aiTimer = AiActionDelaySeconds;
            }

            _aiTimer -= PumpTickSeconds;
            if (_aiTimer <= 0)
                ActForAi(actor);
        }

        /// <summary>Leaves the battle: detaches events and returns to the main menu.</summary>
        public void Leave()
        {
            controller.Events.StateChanged -= Refresh;
            controller.Events.PopupShown -= OnPopupShown;
            popupTimer.Stop();
            onLeave();
        }

        private void ActForAi(ICombatUnit actor)
        {
            string payload = DecideAiPayload(actor);
            if (string.IsNullOrEmpty(payload))
                payload = DuelPayload.PassAction();
            ApplyPayload(payload);
            CompleteAction();
        }

        private string DecideAiPayload(ICombatUnit actor)
        {
            if (actor.Character.Brain != null)
            {
                MonsterBrainDecision decision = controller.Solver.UseMonsterBrain(actor);
                if (decision.Decision == BrainDecisionType.Perform &&
                    decision.SelectedSkill != null &&
                    decision.TargetInfo.Targets.Count > 0)
                {
                    return DuelPayload.Skill(
                        decision.SelectedSkill.Id,
                        decision.TargetInfo.Targets[0].CombatInfo.CombatId);
                }

                return DuelPayload.PassAction();
            }

            return new DuelAi().ChooseAction(controller);
        }

        private void ApplyPayload(string payload)
        {
            string[] parts = payload.Split('|');
            if (parts.Length < 1 || parts[0] == DuelPayload.Pass)
            {
                controller.ExecuteLocalPass();
                return;
            }

            if (parts[0] == DuelPayload.Move)
            {
                int rank;
                if (parts.Length == 2 && int.TryParse(parts[1], out rank))
                {
                    if (controller.ExecuteLocalMove(rank) == null)
                        controller.ExecuteLocalPass();
                }

                return;
            }

            if (parts.Length == 2)
            {
                int targetId;
                if (int.TryParse(parts[1], out targetId))
                {
                    if (controller.ExecuteLocalSkill(parts[0], targetId) == null)
                        controller.ExecuteLocalPass();
                }
            }
        }

        private void CompleteAction()
        {
            RenderSkillResult();
            Refresh();
        }

        /// <summary>Rebuilds the snapshot from the core controller.</summary>
        public void Refresh()
        {
            if (controller.IsFinished)
                AnnounceWinner();
            RefreshUnits();
            RefreshSkills();
            RefreshStatus();
            RefreshLog();
            RefreshEvents();
            RefreshActor();
            ApplyPopups();
        }

        private void RefreshUnits()
        {
            Heroes.Clear();
            foreach (var unit in heroOrder.OrderLeftToRight(controller.HeroParty))
                Heroes.Add(ToUnit(unit, isEnemy: false));
            Monsters.Clear();
            foreach (var unit in monsterOrder.OrderLeftToRight(controller.MonsterParty))
                Monsters.Add(ToUnit(unit, isEnemy: true));

            if (IsBuffTableVisible && BuffTarget != null)
                BuffTarget = Heroes.Concat(Monsters).FirstOrDefault(card => card.CombatId == BuffTarget.CombatId);

            RefreshTurnOrder();
        }

        private void RefreshTurnOrder()
        {
            TurnOrder.Clear();
            if (controller.BattleGround == null || controller.CurrentUnit == null)
                return;

            int round = controller.BattleGround.Round.RoundNumber;
            if (round != _lastRound)
                _lastRound = round;

            int currentId = controller.CurrentUnit.CombatInfo.CombatId;
            foreach (var unit in controller.BattleGround.Round.OrderedUnits)
            {
                if (unit.CombatInfo.IsDead)
                    continue;
                TurnOrder.Add(new DuelTurnEntryViewModel(
                    unit.Character.Name,
                    unit.Team == Team.Monsters,
                    (int)unit.Character.Speed)
                {
                    IsCurrent = unit.CombatInfo.CombatId == currentId,
                });
            }

            if (TurnOrder.Count == 0 || !TurnOrder.Any(entry => entry.IsCurrent))
            {
                var current = controller.CurrentUnit;
                TurnOrder.Insert(0, new DuelTurnEntryViewModel(
                    current.Character.Name,
                    current.Team == Team.Monsters,
                    (int)current.Character.Speed)
                {
                    IsCurrent = true,
                });
            }
        }

        private void RefreshSkills()
        {
            Skills.Clear();
            selectedSkill = null;
            selectedSkillId = null;
            OnPropertyChanged(nameof(SelectedSkill));

            var unit = controller.CurrentUnit;
            if (unit == null)
                return;

            bool playerTurn = IsLocalTurn;
            foreach (var skill in unit.Character.CurrentCombatSkills ?? Enumerable.Empty<CombatSkill>())
                Skills.Add(new DuelSkillViewModel(skill.Id, Ui.DisplayNames.Title(skill.Id), Ui.SkillToneClassifier.Classify(skill))
                {
                    IsUsable = playerTurn && controller.IsSkillUsable(unit, skill),
                    Level = skill.Level,
                    BaseInfo = Ui.SkillDetails.BuildBaseInfo(skill),
                    EffectRows = Ui.SkillDetails.BuildEffectRows(skill),
                    Details = Ui.SkillDetails.Build(skill),
                });
        }

        private void RefreshStatus()
        {
            if (controller.IsFinished)
                Status = _winnerText.Length > 0
                    ? _winnerText + ". Round " + controller.BattleGround!.Round.RoundNumber + "."
                    : "Battle finished. Round " + controller.BattleGround!.Round.RoundNumber + ".";
            else if (IsLocalTurn)
                Status = "Round " + controller.BattleGround!.Round.RoundNumber + " — your turn.";
            else
                Status = "Round " + controller.BattleGround!.Round.RoundNumber + " — enemy turn.";
        }

        private void RefreshLog()
        {
            while (controller.Events.Log.Count > Log.Count)
                Log.Add(controller.Events.Log[Log.Count]);
        }

        private void RefreshEvents()
        {
            int round = controller.BattleGround?.Round.RoundNumber ?? 1;
            Events.Round = round;
            Torch.Round = round;
        }

        private void RefreshActor()
        {
            var unit = controller.CurrentUnit;
            if (unit == null)
                return;

            var character = unit.Character;
            RaidHud.ApplyActor(
                character.Name,
                Ui.DisplayNames.Class(character.Class),
                character.CurrentCombatSkills ?? Enumerable.Empty<CombatSkill>(),
                (int)character.GetPairedAttribute(AttributeType.HitPoints).CurrentValue,
                (int)character.GetPairedAttribute(AttributeType.HitPoints).ModifiedValue,
                character.Stress != null ? (int)character.Stress.CurrentValue : 0,
                (int)character.Speed,
                (int)character.MinDamage,
                (int)character.MaxDamage,
                (int)character.Accuracy,
                (int)(character.Crit * 100),
                (int)character.Dodge,
                (int)character.Protection,
                character is Hero hero ? hero.EquippedTrinketIds : null);
        }

        private void RenderSkillResult()
        {
            SkillResult result = controller.Solver.SkillResult;
            if (result == null)
                return;

            foreach (SkillResultEntry entry in result.SkillEntries)
            {
                if (entry.Target == null || entry.Target.CombatInfo == null)
                    continue;

                string? text = FormatEntry(entry);
                if (text != null)
                    AddPopup(entry.Target.CombatInfo.CombatId, text);
            }
        }

        private static string? FormatEntry(SkillResultEntry entry)
        {
            switch (entry.Type)
            {
                case SkillResultType.Miss:
                    return "MISS";
                case SkillResultType.Dodge:
                    return "DODGE";
                case SkillResultType.Hit:
                    return entry.Amount.ToString();
                case SkillResultType.Crit:
                    return "CRIT!\n" + entry.Amount;
                case SkillResultType.Heal:
                    return "+" + entry.Amount;
                case SkillResultType.CritHeal:
                    return "CRIT HEAL\n+" + entry.Amount;
                default:
                    return null;
            }
        }

        private void OnPopupShown(ICombatUnit target, PopupType type, string value)
        {
            if (target == null || target.CombatInfo == null)
                return;

            string? label = StatusPopupLabel(type, value);
            if (label != null)
                AddPopup(target.CombatInfo.CombatId, label);
        }

        private static string? StatusPopupLabel(PopupType type, string value)
        {
            switch (type)
            {
                case PopupType.Buff:
                    return "BUFF";
                case PopupType.Debuff:
                    return "DEBUFF";
                case PopupType.Bleed:
                    return "BLEED";
                case PopupType.Poison:
                    return "BLIGHT";
                case PopupType.Stunned:
                    return "STUN";
                case PopupType.Tagged:
                    return "MARK";
                case PopupType.Riposte:
                    return "RIPOSTE";
                case PopupType.Guard:
                    return "GUARD";
                case PopupType.Stress:
                    return value == null ? "STRESS" : "STRESS " + value;
                default:
                    return null;
            }
        }

        private void AddPopup(int combatId, string text)
        {
            pendingPopups.Add((combatId, text));
        }

        private void ApplyPopups()
        {
            foreach (var popup in pendingPopups)
            {
                var card = Heroes.FirstOrDefault(h => h.CombatId == popup.CombatId)
                    ?? Monsters.FirstOrDefault(m => m.CombatId == popup.CombatId);
                if (card == null)
                    continue;

                if (!popupQueues.TryGetValue(popup.CombatId, out var queue))
                {
                    queue = new Queue<string>();
                    popupQueues[popup.CombatId] = queue;
                }

                queue.Enqueue(popup.Text);
                if (queue.Count == 1)
                {
                    card.DamagePopupText = popup.Text;
                    card.DamagePopupVisible = true;
                }
            }

            pendingPopups.Clear();
        }

        private void AdvancePopups()
        {
            foreach (var entry in popupQueues.ToList())
            {
                var queue = entry.Value;
                var card = Heroes.FirstOrDefault(h => h.CombatId == entry.Key)
                    ?? Monsters.FirstOrDefault(m => m.CombatId == entry.Key);

                queue.Dequeue();
                if (queue.Count > 0 && card != null)
                {
                    card.DamagePopupText = queue.Peek();
                    card.DamagePopupVisible = false;
                    card.DamagePopupVisible = true;
                }
                else
                {
                    if (card != null)
                        card.DamagePopupVisible = false;
                    popupQueues.Remove(entry.Key);
                }
            }
        }

        private void AnnounceWinner()
        {
            if (_winnerAnnounced)
                return;
            _winnerAnnounced = true;

            bool heroesAlive = controller.HeroParty.Units.Any(unit => !unit.CombatInfo.IsDead);
            bool monstersAlive = controller.MonsterParty.Units.Any(unit => !unit.CombatInfo.IsDead);
            _winnerText = !heroesAlive ? "MONSTERS WIN" : (!monstersAlive ? "HEROES WIN" : "DRAW");
            Events.AnnouncementTitle = _winnerText;
        }

        private void SelectSkill(DuelSkillViewModel? skill)
        {
            if (skill == null || !IsLocalTurn)
                return;

            isMoveMode = false;
            foreach (var existing in Skills)
                existing.IsSelected = false;
            skill.IsSelected = true;
            selectedSkill = skill;
            selectedSkillId = skill.Id;
            OnPropertyChanged(nameof(SelectedSkill));

            ClearTargets();
            var unit = controller.CurrentUnit;
            var skillDef = unit?.Character.CurrentCombatSkills?.FirstOrDefault(s => s.Id == skill.Id);
            if (unit == null || skillDef == null)
                return;

            var targets = controller.GetAvailableTargets(unit, skillDef);
            foreach (var t in targets)
            {
                var card = Heroes.FirstOrDefault(h => h.CombatId == t.CombatInfo.CombatId);
                if (card != null)
                    card.IsTarget = true;
                card = Monsters.FirstOrDefault(h => h.CombatId == t.CombatInfo.CombatId);
                if (card != null)
                    card.IsTarget = true;
            }
        }

        private void SelectTarget(DuelUnitViewModel? unit)
        {
            if (unit == null || !IsLocalTurn)
                return;

            if (isMoveMode)
            {
                int actorRank = controller.CurrentUnit?.Rank ?? 0;
                if (Math.Abs(unit.Rank - actorRank) != 1)
                    return;

                controller.ApplyRemoteSkill(DuelPayload.MoveAction(unit.Rank));
                isMoveMode = false;
                CompleteAction();
                return;
            }

            if (selectedSkill == null || !unit.IsTarget)
                return;

            controller.ApplyRemoteSkill(DuelPayload.Skill(selectedSkillId!, unit.CombatId));
            CompleteAction();
        }

        private void SelectMove()
        {
            if (!IsLocalTurn || controller.CurrentUnit == null)
                return;

            selectedSkill = null;
            selectedSkillId = null;
            isMoveMode = true;
            OnPropertyChanged(nameof(SelectedSkill));
            foreach (var existing in Skills)
                existing.IsSelected = false;
            ClearTargets();

            var unit = controller.CurrentUnit;
            var allies = unit.Team == Team.Heroes ? Heroes : Monsters;
            foreach (var ally in allies)
            {
                if (Math.Abs(ally.Rank - unit.Rank) == 1)
                    ally.IsTarget = true;
            }
        }

        private void Pass()
        {
            if (!IsLocalTurn)
                return;

            controller.ApplyRemoteSkill(DuelPayload.PassAction());
            CompleteAction();
        }

        private void OpenStats(DuelUnitViewModel? unit)
        {
            if (unit == null)
                return;

            var skills = new List<DuelSkillViewModel>();
            foreach (var skill in unit.CombatSkills)
            {
                skills.Add(new DuelSkillViewModel(skill.Id, Ui.DisplayNames.Title(skill.Id), Ui.SkillToneClassifier.Classify(skill))
                {
                    IsUsable = true,
                    Level = skill.Level,
                    BaseInfo = Ui.SkillDetails.BuildBaseInfo(skill),
                    EffectRows = Ui.SkillDetails.BuildEffectRows(skill),
                    Details = Ui.SkillDetails.Build(skill),
                });
            }

            StatsTarget.Apply(unit, skills);
            IsStatsVisible = true;
        }

        private void ToggleBuffTable(DuelUnitViewModel? unit)
        {
            if (unit == null)
                return;

            if (IsBuffTableVisible && ReferenceEquals(BuffTarget, unit))
                IsBuffTableVisible = false;
            else
            {
                BuffTarget = unit;
                IsBuffTableVisible = true;
            }
        }

        private void ClearTargets()
        {
            foreach (var hero in Heroes)
                hero.IsTarget = false;
            foreach (var monster in Monsters)
                monster.IsTarget = false;
        }

        /// <summary>Whether the hovered unit can show the attack arrow (a valid skill/move target
        /// during the player's turn, other than the acting unit itself).</summary>
        /// <param name="target">The hovered unit card.</param>
        public bool CanShowArrow(DuelUnitViewModel target)
        {
            var current = controller.CurrentUnit;
            return IsLocalTurn
                && current != null
                && target != null
                && target.CombatId != current.CombatInfo.CombatId
                && target.IsTarget
                && (selectedSkill != null || isMoveMode);
        }

        /// <inheritdoc/>
        IEnumerable<DuelUnitViewModel> IDuelBattleViewData.Heroes { get { return Heroes; } }

        /// <inheritdoc/>
        IEnumerable<DuelUnitViewModel> IDuelBattleViewData.Monsters { get { return Monsters; } }

        private DuelUnitViewModel ToUnit(ICombatUnit unit, bool isEnemy)
        {
            var character = unit.Character;
            var hp = character.GetPairedAttribute(AttributeType.HitPoints);
            return new DuelUnitViewModel(
                unit.CombatInfo.CombatId,
                unit.Rank,
                unit.Size,
                character.Name,
                Ui.DisplayNames.Class(character.Class))
            {
                IsEnemy = isEnemy,
                IsOnDeathsDoor = character.AtDeathsDoor,
                HpCurrent = (int)hp.CurrentValue,
                HpMax = (int)hp.ModifiedValue,
                Stress = character.Stress != null ? (int)character.Stress.CurrentValue : 0,
                Speed = (int)character.Speed,
                Damage = (int)character.MinDamage + " - " + (int)character.MaxDamage,
                Accuracy = (int)character.Accuracy,
                Crit = (int)(character.Crit * 100),
                Dodge = (int)character.Dodge,
                Protection = (int)character.Protection,
                ResistStun = (int)(ResistPercent(character, AttributeType.Stun)),
                ResistBlight = (int)(ResistPercent(character, AttributeType.Poison)),
                ResistBleed = (int)(ResistPercent(character, AttributeType.Bleed)),
                ResistDebuff = (int)(ResistPercent(character, AttributeType.Debuff)),
                ResistMove = (int)(ResistPercent(character, AttributeType.Move)),
                ResistDisease = (int)(ResistPercent(character, AttributeType.Disease)),
                ResistDeathBlow = (int)(ResistPercent(character, AttributeType.DeathBlow)),
                ResistTrap = (int)(ResistPercent(character, AttributeType.Trap)),
                Debuffs = BuildDebuffs(character),
                Buffs = BuildBuffs(character),
                CombatSkills = (character.CurrentCombatSkills ?? Enumerable.Empty<CombatSkill>()).ToList(),
            };
        }

        private static float ResistPercent(ICharacter character, AttributeType type)
        {
            var attribute = character.GetSingleAttribute(type);
            return attribute != null ? attribute.ModifiedValue * 100f : 0f;
        }

        private static List<BuffRowViewModel> BuildBuffs(ICharacter character)
        {
            return BuildBuffRows(character, positive: true);
        }

        private static List<BuffRowViewModel> BuildDebuffs(ICharacter character)
        {
            return BuildBuffRows(character, positive: false);
        }

        private static List<BuffRowViewModel> BuildBuffRows(ICharacter character, bool positive)
        {
            var rows = new List<BuffRowViewModel>();
            var source = character as Character;
            if (source != null)
            {
                foreach (var buffInfo in source.BuffInfos)
                {
                    var buff = buffInfo.Buff;
                    bool isPositive = buff.IsPositive();
                    if (isPositive != positive)
                        continue;

                    rows.Add(new BuffRowViewModel(
                        Ui.BuffDetails.FormatName(buff),
                        Ui.BuffDetails.FormatDuration(buffInfo),
                        Ui.BuffDetails.FormatDescription(buff),
                        isPositive ? "Buff" : "Debuff"));
                }
            }

            return rows;
        }
    }
}