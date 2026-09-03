using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Party;
using Sektor.DarkestDungeon.Core.Duel;
using Sektor.DarkestDungeon.Core.Duel.Fight;

/// <summary>
/// The thin battle orchestrator: owns the pure core <see cref="DuelController"/>, builds it from a
/// <see cref="BattleTestConfig"/>, routes player input and AI actions into it and renders the result
/// through the formation views, the popup layer and the HUD. No battle logic lives here.
/// </summary>
public class CoreBattleDriver : MonoBehaviour
{
    private const float AiActionDelay = 0.4f;

    [SerializeField]
    private BattleFormationView heroFormation;

    [SerializeField]
    private BattleFormationView monsterFormation;

    [SerializeField]
    private BattleHud hud;

    [SerializeField]
    private Transform fieldRoot;

    private readonly DuelAi _ai = new DuelAi();

    private DuelController _duel;
    private BattleTestConfig _config;
    private BattlePopupLayer _popupLayer;
    private BattleEventsAdapter _eventsAdapter;
    private bool _started;
    private int _lastActorCombatId = -1;
    private int _lastRound = -1;
    private int _logWatermark;
    private bool _turnInitialized;
    private bool _winnerAnnounced;
    private float _aiTimer;

    /// <summary>Gets the running core duel controller (null before <see cref="Begin"/>).</summary>
    public DuelController Duel { get { return _duel; } }

    /// <summary>Gets a value indicating whether the battle has been started.</summary>
    public bool IsStarted { get { return _started; } }

    /// <summary>Gets a value indicating whether the battle has finished.</summary>
    public bool IsFinished { get { return _duel != null && _duel.IsFinished; } }

    /// <summary>Builds and starts a battle from the given configuration.</summary>
    /// <param name="config">The battle configuration.</param>
    public void Begin(BattleTestConfig config)
    {
        _config = config;
        _started = false;
        _winnerAnnounced = false;
        _lastActorCombatId = -1;
        _lastRound = -1;
        _logWatermark = 0;
        _turnInitialized = false;

        _duel = new DuelController(FightContentLoader.Content);
        bool opponentHasMonsters = config.Side2.Slots.Any(slot => slot != null && !slot.IsHero);
        if (opponentHasMonsters)
            _duel.StartFight(ToFightSpecs(config.Side1, config.Seed), ToFightSpecs(config.Side2, config.Seed), config.Seed);
        else
            _duel.StartDuel(ToPicks(config.Side1, config.Seed), ToPicks(config.Side2, config.Seed), config.Seed, true);

        _duel.Context.TorchAmount = Mathf.Clamp(config.Torch, 0, 100);
        _duel.StartBattle();

        _popupLayer = BattlePopupLayer.Create(fieldRoot);
        _popupLayer.WorldPosResolver = ResolveWorldPosition;
        _eventsAdapter = new BattleEventsAdapter(_popupLayer);
        _eventsAdapter.Attach(_duel);

        hud.Bind(this);
        hud.Clear();

        heroFormation.Initialize(_duel.HeroParty, FormationDisplayOrder.HeroSide(),
            new Vector3(-4.2f, 0f, 0f), 1f);
        monsterFormation.Initialize(_duel.MonsterParty, FormationDisplayOrder.MonsterSide(),
            new Vector3(0.8f, 0f, 0f), 1f);

        RefreshView();
        _started = true;
    }

    /// <summary>Executes a player skill against a target combat id.</summary>
    /// <param name="skillId">The skill id.</param>
    /// <param name="targetCombatId">The target combat id.</param>
    public void PlayerAct(string skillId, int targetCombatId)
    {
        if (!CanPlayerAct())
            return;

        if (_duel.IsLocalTurn)
        {
            if (_duel.ExecuteLocalSkill(skillId, targetCombatId) != null)
                CompleteAction();
        }
        else
        {
            _duel.ApplyRemoteSkill(DuelPayload.Skill(skillId, targetCombatId));
            CompleteAction();
        }
    }

    /// <summary>Executes a player pass.</summary>
    public void PlayerPass()
    {
        if (!CanPlayerAct())
            return;

        if (_duel.IsLocalTurn)
            _duel.ExecuteLocalPass();
        else
            _duel.ApplyRemoteSkill(DuelPayload.PassAction());
        CompleteAction();
    }

    /// <summary>Executes a player move of the acting unit to an adjacent rank.</summary>
    /// <param name="newRank">The destination rank.</param>
    public void PlayerMove(int newRank)
    {
        if (!CanPlayerAct())
            return;

        if (_duel.IsLocalTurn)
        {
            if (_duel.ExecuteLocalMove(newRank) != null)
                CompleteAction();
        }
        else
        {
            _duel.ApplyRemoteSkill(DuelPayload.MoveAction(newRank));
            CompleteAction();
        }
    }

    private void Update()
    {
        if (!_started || _duel == null)
            return;

        if (_duel.IsFinished)
        {
            AnnounceWinner();
            return;
        }

        ICombatUnit actor = _duel.CurrentUnit;
        if (actor == null)
            return;

        if (actor.CombatInfo.CombatId != _lastActorCombatId)
        {
            _lastActorCombatId = actor.CombatInfo.CombatId;
            _turnInitialized = false;
            hud.Announce(string.Empty);
        }

        if (IsPlayerControlled(actor))
        {
            if (!_turnInitialized)
            {
                _turnInitialized = true;
                hud.ShowActor(actor);
            }
            return;
        }

        if (!_turnInitialized)
        {
            _turnInitialized = true;
            _aiTimer = AiActionDelay;
        }

        _aiTimer -= Time.deltaTime;
        if (_aiTimer <= 0f)
            ActForAi(actor);
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
        if (actor.Character.IsMonster && actor.Character.Brain != null)
        {
            Sektor.DarkestDungeon.Core.Combat.Mechanics.AI.MonsterBrainDecision decision = _duel.Solver.UseMonsterBrain(actor);
            if (decision.Decision == Sektor.DarkestDungeon.Core.Combat.Mechanics.AI.BrainDecisionType.Perform &&
                decision.SelectedSkill != null &&
                decision.TargetInfo.Targets.Count > 0)
            {
                return DuelPayload.Skill(
                    decision.SelectedSkill.Id,
                    decision.TargetInfo.Targets[0].CombatInfo.CombatId);
            }

            return DuelPayload.PassAction();
        }

        return _ai.ChooseAction(_duel);
    }

    private void ApplyPayload(string payload)
    {
        if (!_duel.IsLocalTurn)
        {
            _duel.ApplyRemoteSkill(payload);
            return;
        }

        string[] parts = payload.Split('|');
        if (parts.Length < 1 || parts[0] == DuelPayload.Pass)
        {
            _duel.ExecuteLocalPass();
            return;
        }

        if (parts[0] == DuelPayload.Move)
        {
            int rank;
            if (parts.Length == 2 && int.TryParse(parts[1], out rank))
            {
                if (_duel.ExecuteLocalMove(rank) == null)
                    _duel.ExecuteLocalPass();
            }
            return;
        }

        if (parts.Length == 2)
        {
            int targetId;
            if (int.TryParse(parts[1], out targetId))
            {
                if (_duel.ExecuteLocalSkill(parts[0], targetId) == null)
                    _duel.ExecuteLocalPass();
            }
        }
    }

    private void CompleteAction()
    {
        RenderSkillResult();
        RefreshView();
    }

    private void RefreshView()
    {
        if (_duel == null)
            return;

        heroFormation.UpdateUnits();
        monsterFormation.UpdateUnits();

        int round = _duel.BattleGround.Round.RoundNumber;
        if (round != _lastRound)
        {
            _lastRound = round;
            hud.SetRound(round);
        }

        DrainLog();
    }

    private void RenderSkillResult()
    {
        Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle.SkillResult result = _duel.Solver.SkillResult;
        if (result == null || _popupLayer == null)
            return;

        foreach (Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle.SkillResultEntry entry in result.SkillEntries)
        {
            if (entry.Target == null || entry.Target.CombatInfo == null)
                continue;

            string text;
            Color color;
            if (!TryFormatEntry(entry, out text, out color))
                continue;

            _popupLayer.ShowAt(entry.Target.CombatInfo.CombatId, text, color);
        }
    }

    private static bool TryFormatEntry(Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle.SkillResultEntry entry, out string text, out Color color)
    {
        text = string.Empty;
        color = Color.white;

        switch (entry.Type)
        {
            case Sektor.DarkestDungeon.Core.Combat.Mechanics.SkillResultType.Miss:
                text = "MISS";
                color = new Color(0.9f, 0.9f, 0.9f);
                return true;
            case Sektor.DarkestDungeon.Core.Combat.Mechanics.SkillResultType.Dodge:
                text = "DODGE";
                color = new Color(0.9f, 0.9f, 0.9f);
                return true;
            case Sektor.DarkestDungeon.Core.Combat.Mechanics.SkillResultType.Hit:
                text = entry.Amount.ToString();
                color = new Color(1f, 0.35f, 0.3f);
                return true;
            case Sektor.DarkestDungeon.Core.Combat.Mechanics.SkillResultType.Crit:
                text = "CRIT!\n" + entry.Amount;
                color = new Color(1f, 0.65f, 0.2f);
                return true;
            case Sektor.DarkestDungeon.Core.Combat.Mechanics.SkillResultType.Heal:
                text = "+" + entry.Amount;
                color = new Color(0.4f, 1f, 0.4f);
                return true;
            case Sektor.DarkestDungeon.Core.Combat.Mechanics.SkillResultType.CritHeal:
                text = "CRIT HEAL\n+" + entry.Amount;
                color = new Color(0.4f, 1f, 0.4f);
                return true;
            default:
                return false;
        }
    }

    private void DrainLog()
    {
        var log = _duel.Events.Log;
        for (int i = _logWatermark; i < log.Count; i++)
            hud.AppendLog(log[i]);
        _logWatermark = log.Count;
    }

    private void AnnounceWinner()
    {
        if (_winnerAnnounced)
            return;
        _winnerAnnounced = true;

        bool heroesAlive = _duel.HeroParty.Units.Any(unit => !unit.CombatInfo.IsDead);
        bool monstersAlive = _duel.MonsterParty.Units.Any(unit => !unit.CombatInfo.IsDead);
        string winner = !heroesAlive ? "MONSTERS WIN" : (!monstersAlive ? "HEROES WIN" : "DRAW");
        hud.HideActor();
        hud.ShowWinner(winner);
    }

    private bool CanPlayerAct()
    {
        return _started && _duel != null && !_duel.IsFinished &&
            _duel.CurrentUnit != null && IsPlayerControlled(_duel.CurrentUnit);
    }

    private bool IsPlayerControlled(ICombatUnit unit)
    {
        bool side1 = unit.Team == Sektor.DarkestDungeon.Core.Combat.Raid.Battle.Team.Heroes;
        return side1 ? _config.PlayerControlsSide1 : _config.PlayerControlsSide2;
    }

    private Vector3 ResolveWorldPosition(int combatId)
    {
        BattleUnitView view = heroFormation.GetView(combatId);
        if (view == null)
            view = monsterFormation.GetView(combatId);
        return view != null ? view.WorldPosition : Vector3.zero;
    }

    private static List<FightUnitSpec> ToFightSpecs(BattleTestSideSpec side, int seedBase)
    {
        var specs = new List<FightUnitSpec>();
        for (int i = 0; i < side.Slots.Count; i++)
        {
            BattleTestSlotSpec slot = side.Slots[i];
            if (slot == null || string.IsNullOrEmpty(slot.ClassId))
                continue;

            if (slot.IsHero)
                specs.Add(new HeroFightUnitSpec(slot.ClassId, seedBase + i + 1, slot.SkillIds, slot.QuirkIds, slot.TrinketIds));
            else
                specs.Add(new MonsterFightUnitSpec(slot.ClassId));
        }
        return specs;
    }

    private static List<DuelHeroPick> ToPicks(BattleTestSideSpec side, int seedBase)
    {
        var picks = new List<DuelHeroPick>();
        for (int i = 0; i < side.Slots.Count; i++)
        {
            BattleTestSlotSpec slot = side.Slots[i];
            if (slot == null || string.IsNullOrEmpty(slot.ClassId) || !slot.IsHero)
                continue;
            picks.Add(new DuelHeroPick(slot.ClassId, seedBase + i + 1, slot.SkillIds, slot.QuirkIds, slot.TrinketIds));
        }
        return picks;
    }

    private void OnDestroy()
    {
        if (_eventsAdapter != null && _duel != null)
            _eventsAdapter.Detach(_duel);
    }
}