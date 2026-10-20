using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
// Unity 7 injects implicit global usings and its own assemblies contain overlapping type names
// (global StateSnapshot, VectorGraphics.SceneNode, ...); aliases pin every contract type used
// here to the recording DTOs instead of the engine ones.
using AnimationRef = Sektor.DarkestDungeon.Core.GameEvents.AnimationRef;
using AttackExecuted = Sektor.DarkestDungeon.Core.GameEvents.AttackExecuted;
using AudioActions = Sektor.DarkestDungeon.Core.GameEvents.AudioActions;
using AudioEventKinds = Sektor.DarkestDungeon.Core.GameEvents.AudioEventKinds;
using AudioParameter = Sektor.DarkestDungeon.Core.GameEvents.AudioParameter;
using AudioPlayed = Sektor.DarkestDungeon.Core.GameEvents.AudioPlayed;
using AudioPosition = Sektor.DarkestDungeon.Core.GameEvents.AudioPosition;
using BattleEnded = Sektor.DarkestDungeon.Core.GameEvents.BattleEnded;
using BattleState = Sektor.DarkestDungeon.Core.GameEvents.BattleState;
using BattleWinners = Sektor.DarkestDungeon.Core.GameEvents.BattleWinners;
using CombatantBuffState = Sektor.DarkestDungeon.Core.GameEvents.CombatantBuffState;
using CombatantState = Sektor.DarkestDungeon.Core.GameEvents.CombatantState;
using CombatantStatusState = Sektor.DarkestDungeon.Core.GameEvents.CombatantStatusState;
using GameInput = Sektor.DarkestDungeon.Core.GameEvents.GameInput;
using GameInputKinds = Sektor.DarkestDungeon.Core.GameEvents.GameInputKinds;
using GameRng = Sektor.DarkestDungeon.Core.GameEvents.GameRng;
using HeroStatChanged = Sektor.DarkestDungeon.Core.GameEvents.HeroStatChanged;
using PartyMoved = Sektor.DarkestDungeon.Core.GameEvents.PartyMoved;
using RoomDiscovered = Sektor.DarkestDungeon.Core.GameEvents.RoomDiscovered;
using RoundEnded = Sektor.DarkestDungeon.Core.GameEvents.RoundEnded;
using RoundStarted = Sektor.DarkestDungeon.Core.GameEvents.RoundStarted;
using SceneGlobals = Sektor.DarkestDungeon.Core.GameEvents.SceneGlobals;
using SceneNode = Sektor.DarkestDungeon.Core.GameEvents.SceneNode;
// SceneSnapshot is deliberately NOT aliased: the diagnostics namespace owns a global
// SceneSnapshot (the scene-dumper DTO); the one event usage below is fully qualified.
using SkillResolved = Sektor.DarkestDungeon.Core.GameEvents.SkillResolved;
using SkillTargetResult = Sektor.DarkestDungeon.Core.GameEvents.SkillTargetResult;
using StateSnapshot = Sektor.DarkestDungeon.Core.GameEvents.StateSnapshot;
using StateSnapshotContexts = Sektor.DarkestDungeon.Core.GameEvents.StateSnapshotContexts;
using SubsystemNames = Sektor.DarkestDungeon.Core.GameEvents.SubsystemNames;
using TorchChanged = Sektor.DarkestDungeon.Core.GameEvents.TorchChanged;
using TurnEnded = Sektor.DarkestDungeon.Core.GameEvents.TurnEnded;
using TurnStarted = Sektor.DarkestDungeon.Core.GameEvents.TurnStarted;
using UnitMoved = Sektor.DarkestDungeon.Core.GameEvents.UnitMoved;

/// <summary>
/// Static facade for the recorded game-event pipeline (docs\SPEC_GAME_EVENTS_SCHEMA.md): thin
/// helpers called from the minimal instrumentation points in the legacy raid code. Everything is
/// a no-op until <see cref="GameEventRecorder"/> starts a recording, so the raid path pays only a
/// null-check when nothing is being recorded.
/// </summary>
public static class GameEventRecording
{
    private const string HeroIdPrefix = "hero_";
    private const string MonsterIdPrefix = "monster_";
    private const string PortraitSuffix = "_portrait";
    private const string HpStat = "hp";
    private const string HpInvariant = "newValue >= 0";
    private const string StressStat = "stress";
    private const string StressInvariant = "newValue >= 0 && newValue <= maxStress";
    private const string SkillMoveCause = "skill";
    private const string PartyRootId = "party_root";
    private const string PartyFormationComponent = "PartyFormation";
    private const string HeroViewComponent = "HeroView";
    private const string SkeletonAnimationComponent = "SkeletonAnimation";
    private const string MusicEventPrefix = "event:/music/";
    private const string AmbienceEventPrefix = "event:/ambience/";
    private const string VoiceEventPrefix = "event:/vo/";
    private const string UiEventPrefix = "event:/ui/";
    private const string PropsEventPrefix = "event:/props/";
    private const string CampEventPrefix = "event:/camp/";
    private const string TownEventPrefix = "event:/town/";

    /// <summary>Publishes the raid-start scene snapshot (party, globals, hierarchy).</summary>
    public static void PublishSceneSnapshot()
    {
        GameEventRecorder recorder = ActiveRecorder();
        if (recorder == null)
            return;

        IList<FormationUnit> units = RaidSceneManager.HeroParty.Units;
        var hierarchy = new List<SceneNode>(units.Count + 1);
        var heroes = new List<CombatantState>(units.Count);
        var party = new List<string>(units.Count);

        hierarchy.Add(new SceneNode(PartyRootId, null, new List<string> { PartyFormationComponent }));
        for (int i = 0; i < units.Count; i++)
        {
            FormationUnit unit = units[i];
            string id = UnitId(unit);
            hierarchy.Add(new SceneNode(id, PartyRootId,
                new List<string> { HeroViewComponent, SkeletonAnimationComponent }));
            heroes.Add(ToCombatant(unit));
            party.Add(id);
        }

        recorder.Publish(new Sektor.DarkestDungeon.Core.GameEvents.SceneSnapshot(
            SceneManager.GetActiveScene().name,
            hierarchy,
            CurrentGlobals(),
            heroes,
            party));
    }

    /// <summary>Publishes a party movement between two map locations (room/sector ids).</summary>
    /// <param name="fromId">The source location id; empty/null and self-moves are skipped.</param>
    /// <param name="toId">The destination location id.</param>
    public static void PublishPartyMoved(string fromId, string toId)
    {
        if (string.IsNullOrEmpty(fromId) || string.IsNullOrEmpty(toId) || fromId == toId)
            return;

        GameEventRecorder recorder = ActiveRecorder();
        if (recorder == null)
            return;

        recorder.Publish(new PartyMoved(fromId, toId));
    }

    /// <summary>Publishes the discovery of a room entered for the first time (knowledge Hidden).</summary>
    /// <param name="room">The room being entered.</param>
    public static void PublishRoomDiscovered(DungeonRoom room)
    {
        if (room == null || room.Knowledge != Knowledge.Hidden)
            return;

        GameEventRecorder recorder = ActiveRecorder();
        if (recorder == null)
            return;

        recorder.Publish(new RoomDiscovered(room.Id, room.Type.ToString()));
    }

    /// <summary>Publishes the battle-start snapshot with both sides of the encounter.</summary>
    public static void PublishBattleStart()
    {
        PublishSnapshot(StateSnapshotContexts.BattleStart);
    }

    /// <summary>Publishes the battle-end event and the final battle snapshot.</summary>
    public static void PublishBattleEnded()
    {
        GameEventRecorder recorder = ActiveRecorder();
        if (recorder == null || RaidSceneManager.BattleGround == null)
            return;

        BattleGround battleGround = RaidSceneManager.BattleGround;
        bool heroesAlive = HasLivingUnits(battleGround.HeroParty.Units);
        bool monstersAlive = HasLivingUnits(battleGround.MonsterParty.Units);
        string winner = !monstersAlive ? BattleWinners.Heroes
            : !heroesAlive ? BattleWinners.Monsters
            : BattleWinners.Unknown;

        int rounds = battleGround.Round != null ? battleGround.Round.RoundNumber : 0;
        recorder.Publish(new BattleEnded(winner, rounds));
        PublishSnapshot(StateSnapshotContexts.BattleEnd);
    }

    /// <summary>Publishes a turn start and the remaining turn order.</summary>
    /// <param name="actor">The unit whose turn starts.</param>
    /// <param name="round">The current round number.</param>
    /// <param name="orderedUnits">The remaining turn order.</param>
    public static void PublishTurnStarted(FormationUnit actor, int round, IList<FormationUnit> orderedUnits)
    {
        if (actor == null)
            return;

        GameEventRecorder recorder = ActiveRecorder();
        if (recorder == null)
            return;

        recorder.Publish(new TurnStarted(
            UnitId(actor),
            round,
            TurnKind(actor),
            UnitIds(orderedUnits)));
    }

    /// <summary>Publishes a turn end and the turn-end snapshot.</summary>
    /// <param name="actor">The unit whose turn ends.</param>
    /// <param name="round">The current round number.</param>
    public static void PublishTurnEnded(FormationUnit actor, int round)
    {
        if (actor == null)
            return;

        GameEventRecorder recorder = ActiveRecorder();
        if (recorder == null)
            return;

        recorder.Publish(new TurnEnded(UnitId(actor), round));
        PublishSnapshot(StateSnapshotContexts.TurnEnd);
    }

    /// <summary>Publishes a new round start with the full turn order.</summary>
    /// <param name="round">The new round number.</param>
    /// <param name="orderedUnits">The full turn order.</param>
    public static void PublishRoundStarted(int round, IList<FormationUnit> orderedUnits)
    {
        GameEventRecorder recorder = ActiveRecorder();
        if (recorder == null)
            return;

        recorder.Publish(new RoundStarted(round, UnitIds(orderedUnits)));
    }

    /// <summary>Publishes the end of a round.</summary>
    /// <param name="round">The round that ended.</param>
    public static void PublishRoundEnded(int round)
    {
        GameEventRecorder recorder = ActiveRecorder();
        if (recorder == null)
            return;

        recorder.Publish(new RoundEnded(round));
    }

    /// <summary>Captures formation ranks before a skill so movement can be recorded exactly.</summary>
    /// <returns>Unit-to-rank map for both parties; empty outside a battle.</returns>
    public static Dictionary<FormationUnit, int> CaptureRanks()
    {
        var ranks = new Dictionary<FormationUnit, int>();
        if (RaidSceneManager.Instanse == null || RaidSceneManager.BattleGround == null)
            return ranks;

        AddRanks(RaidSceneManager.BattleGround.HeroParty.Units, ranks);
        AddRanks(RaidSceneManager.BattleGround.MonsterParty.Units, ranks);
        return ranks;
    }

    /// <summary>
    /// Publishes an hp stat change of a skill target when the executed skill actually moved its
    /// health (miss/dodge publish nothing); called right after BattleSolver.ExecuteSkill.
    /// </summary>
    /// <param name="target">The skill target unit.</param>
    /// <param name="healthBefore">The target's hp captured before the skill executed.</param>
    /// <param name="stressBefore">The target's stress captured before the skill executed.</param>
    /// <param name="entry">The result entry of this target.</param>
    public static void PublishSkillResult(
        FormationUnit target,
        float healthBefore,
        float stressBefore,
        SkillResultEntry entry)
    {
        if (target == null || entry == null)
            return;

        GameEventRecorder recorder = ActiveRecorder();
        if (recorder == null)
            return;

        int oldHp = Mathf.RoundToInt(healthBefore);
        int newHp = Mathf.RoundToInt(target.Character.CurrentHealth);
        if (oldHp != newHp)
        {
            recorder.Publish(new HeroStatChanged(
                UnitId(target), HpStat, oldHp, newHp, entry.Type.ToString(), HpInvariant));
        }

        int oldStress = Mathf.RoundToInt(stressBefore);
        int newStress = Mathf.RoundToInt(target.Character.Stress.CurrentValue);
        if (oldStress != newStress)
        {
            recorder.Publish(new HeroStatChanged(
                UnitId(target), StressStat, oldStress, newStress, entry.Type.ToString(), StressInvariant));
        }
    }

    private static GameRng ToSkillRng(List<SkillResultEntry> entries)
    {
        if (entries == null)
            return null;

        foreach (SkillResultEntry entry in entries)
        {
            if (entry == null)
                continue;

            float? roll = entry.CritRoll ?? entry.HitRoll;
            if (!roll.HasValue)
                return null;

            float? modifier = entry.CritRoll.HasValue ? entry.CritChance : entry.HitChance;
            bool crit = entry.Type == SkillResultType.Crit || entry.Type == SkillResultType.CritHeal;
            return new GameRng(Roll: roll, Crit: crit, Modifier: modifier);
        }

        return null;
    }

    /// <summary>Publishes the exact skill outcome for every target and the resulting rank moves.</summary>
    /// <param name="performer">The skill performer.</param>
    /// <param name="targetInfo">The resolved skill targets.</param>
    /// <param name="result">The solver result with one entry per target.</param>
    /// <param name="ranksBefore">Ranks captured before execution (see <see cref="CaptureRanks"/>).</param>
    public static void PublishSkillResolved(
        FormationUnit performer,
        SkillTargetInfo targetInfo,
        SkillResult result,
        Dictionary<FormationUnit, int> ranksBefore)
    {
        if (performer == null || targetInfo == null || targetInfo.Skill == null || result == null)
            return;

        GameEventRecorder recorder = ActiveRecorder();
        if (recorder == null)
            return;

        var targets = new List<SkillTargetResult>();
        if (result.SkillEntries != null)
        {
            foreach (SkillResultEntry entry in result.SkillEntries)
            {
                if (entry == null || entry.Target == null)
                    continue;

                targets.Add(new SkillTargetResult(
                    UnitId(entry.Target),
                    entry.Type.ToString(),
                    entry.Amount,
                    entry.IsTargetHit,
                    entry.IsZeroed,
                    entry.IsHarmful,
                    Mathf.RoundToInt(entry.Target.Character.CurrentHealth),
                    Mathf.RoundToInt(entry.Target.Character.Stress.CurrentValue),
                    entry.Target.Rank));
            }
        }

        var effects = new List<string>();
        if (result.AppliedEffects != null)
        {
            foreach (Effect effect in result.AppliedEffects)
            {
                if (effect != null)
                    effects.Add(effect.Name);
            }
        }

        recorder.Publish(new SkillResolved(
            UnitId(performer),
            targetInfo.Skill.Id,
            targets,
            effects,
            null,
            ToSkillRng(result.SkillEntries)));

        if (ranksBefore == null)
            return;

        foreach (KeyValuePair<FormationUnit, int> rank in ranksBefore)
        {
            if (rank.Key == null || rank.Key.Rank == rank.Value)
                continue;

            recorder.Publish(new UnitMoved(
                UnitId(rank.Key),
                rank.Value,
                rank.Key.Rank,
                SkillMoveCause));
        }
    }

    /// <summary>
    /// Publishes an executed skill with the attacker's actual playing animation, read a frame after
    /// the animation intro (Animation Events fire on the clip's first frames). The target animation
    /// stays null in v1 - units have no hit-reaction skeletons (schema §3.5).
    /// </summary>
    /// <param name="performer">The attacking unit.</param>
    /// <param name="targetInfo">The resolved skill targets.</param>
    /// <param name="byAi">True for monster actions (input kind aiDecision).</param>
    public static void PublishAttack(FormationUnit performer, SkillTargetInfo targetInfo, bool byAi)
    {
        if (performer == null || targetInfo == null || targetInfo.Skill == null)
            return;

        GameEventRecorder recorder = ActiveRecorder();
        if (recorder == null)
            return;

        FormationUnit primary = null;
        if (targetInfo.Targets != null && targetInfo.Targets.Count > 0)
            primary = targetInfo.Targets[0];

        string targetId = primary != null ? UnitId(primary) : string.Empty;
        recorder.Publish(new AttackExecuted(
            UnitId(performer),
            targetId,
            targetInfo.Skill.Id,
            CurrentAnimation(performer),
            null,
            new GameInput(
                byAi ? GameInputKinds.AiDecision : GameInputKinds.PlayerClick,
                targetId,
                targetInfo.Skill.Id)));
    }

    /// <summary>Publishes a torch value change with its reason code.</summary>
    /// <param name="oldValue">The torch value before the change.</param>
    /// <param name="newValue">The torch value after the change.</param>
    /// <param name="reason">The reason wire code (corridorStep/snuffed/torchUsed/modifier).</param>
    public static void PublishTorchChanged(int oldValue, int newValue, string reason)
    {
        GameEventRecorder recorder = ActiveRecorder();
        if (recorder == null)
            return;

        recorder.Publish(new TorchChanged(oldValue, newValue, reason));
    }

    /// <summary>Publishes an audio event with the exact runtime path and the requested volume.</summary>
    /// <param name="eventPath">The FMOD event path that was actually requested.</param>
    /// <param name="action">The action performed (see <see cref="AudioActions"/>).</param>
    /// <param name="volume">The requested volume.</param>
    /// <param name="position">The world position for 3D events, or <c>null</c>.</param>
    /// <param name="parameters">FMOD parameters set before starting, or <c>null</c>.</param>
    /// <param name="rng">The chance roll for narration, or <c>null</c>.</param>
    public static void PublishAudio(
        string eventPath,
        string action,
        float volume,
        AudioPosition position = null,
        IReadOnlyList<AudioParameter> parameters = null,
        GameRng rng = null)
    {
        if (string.IsNullOrEmpty(eventPath))
            return;

        GameEventRecorder recorder = ActiveRecorder();
        if (recorder == null)
            return;

        recorder.Publish(new AudioPlayed(
            eventPath,
            AudioKind(eventPath),
            action,
            volume,
            position,
            parameters,
            null,
            rng));
    }

    private static void PublishSnapshot(string context)
    {
        GameEventRecorder recorder = ActiveRecorder();
        if (recorder == null || RaidSceneManager.BattleGround == null)
            return;

        IList<FormationUnit> heroUnits = RaidSceneManager.BattleGround.HeroParty.Units;
        IList<FormationUnit> monsterUnits = RaidSceneManager.BattleGround.MonsterParty.Units;
        var combatants = new List<CombatantState>(heroUnits.Count + monsterUnits.Count);
        for (int i = 0; i < heroUnits.Count; i++)
            combatants.Add(ToCombatant(heroUnits[i]));
        for (int i = 0; i < monsterUnits.Count; i++)
            combatants.Add(ToCombatant(monsterUnits[i]));

        recorder.Publish(new StateSnapshot(
            context,
            CurrentGlobals(),
            combatants,
            SubsystemNames.Combat,
            null,
            null,
            CurrentBattleState()));
    }

    private static BattleState CurrentBattleState()
    {
        if (RaidSceneManager.BattleGround == null || RaidSceneManager.BattleGround.Round == null)
            return null;

        Round round = RaidSceneManager.BattleGround.Round;
        FormationUnit selected = round.SelectedUnit;
        return new BattleState(
            round.RoundNumber,
            selected != null ? TurnKind(selected) : null,
            selected != null ? UnitId(selected) : null,
            UnitIds(round.OrderedUnits),
            RaidSceneManager.BattleGround.SurpriseStatus.ToString(),
            RaidSceneManager.BattleGround.BattleStatus.ToString(),
            round.HeroAction == HeroTurnAction.Retreat);
    }

    private static bool HasLivingUnits(IList<FormationUnit> units)
    {
        if (units == null)
            return false;

        for (int i = 0; i < units.Count; i++)
        {
            FormationUnit unit = units[i];
            if (unit == null || unit.Character == null || unit.CombatInfo == null)
                continue;
            if (!unit.CombatInfo.IsDead && unit.Character.CurrentHealth > 0f)
                return true;
        }

        return false;
    }

    private static void AddRanks(IList<FormationUnit> units, Dictionary<FormationUnit, int> ranks)
    {
        for (int i = 0; i < units.Count; i++)
        {
            if (units[i] != null)
                ranks[units[i]] = units[i].Rank;
        }
    }

    private static List<string> UnitIds(IList<FormationUnit> units)
    {
        var ids = new List<string>();
        if (units == null)
            return ids;

        for (int i = 0; i < units.Count; i++)
        {
            if (units[i] != null)
                ids.Add(UnitId(units[i]));
        }

        return ids;
    }

    private static string TurnKind(FormationUnit unit)
    {
        return unit.Character.IsMonster ? "Monster" : "Hero";
    }

    private static string AudioKind(string eventPath)
    {
        if (eventPath.StartsWith(MusicEventPrefix, StringComparison.Ordinal))
            return AudioEventKinds.Music;
        if (eventPath.StartsWith(AmbienceEventPrefix, StringComparison.Ordinal))
            return AudioEventKinds.Ambience;
        if (eventPath.StartsWith(VoiceEventPrefix, StringComparison.Ordinal))
            return AudioEventKinds.Voice;
        if (eventPath.StartsWith(UiEventPrefix, StringComparison.Ordinal))
            return AudioEventKinds.Ui;
        if (eventPath.StartsWith(PropsEventPrefix, StringComparison.Ordinal))
            return AudioEventKinds.Props;
        if (eventPath.StartsWith(CampEventPrefix, StringComparison.Ordinal))
            return AudioEventKinds.Camp;
        if (eventPath.StartsWith(TownEventPrefix, StringComparison.Ordinal))
            return AudioEventKinds.Town;
        return AudioEventKinds.Sfx;
    }

    private static GameEventRecorder ActiveRecorder()
    {
        GameEventRecorder recorder = GameEventRecorder.Instance;
        if (recorder == null || !recorder.IsRecording || RaidSceneManager.Instanse == null)
            return null;
        return recorder;
    }

    private static SceneGlobals CurrentGlobals()
    {
        return new SceneGlobals(
            RaidSceneManager.TorchMeter.TorchAmount,
            DarkestDungeonManager.SaveData.GoldAmount);
    }

    private static string UnitId(FormationUnit unit)
    {
        Hero hero = unit.Character as Hero;
        if (hero != null)
            return HeroIdPrefix + hero.RosterId;
        return MonsterIdPrefix + unit.CombatInfo.CombatId;
    }

    private static CombatantState ToCombatant(FormationUnit unit)
    {
        Character character = unit.Character;
        return new CombatantState(
            UnitId(unit),
            character.Class,
            Mathf.RoundToInt(character.CurrentHealth),
            Mathf.RoundToInt(character.MaxHealth),
            character.IsMonster,
            character.Class + PortraitSuffix,
            character.Class,
            unit.Rank,
            unit.Size,
            Mathf.RoundToInt(character.Stress.CurrentValue),
            Mathf.RoundToInt(character.Stress.ModifiedValue),
            character[StatusType.DeathsDoor].IsApplied,
            unit.CombatInfo.IsDead || character.CurrentHealth <= 0f,
            character.IsAfflicted,
            character.IsVirtued,
            ToStatuses(character),
            ToBuffs(character));
    }

    private static IReadOnlyList<CombatantStatusState> ToStatuses(Character character)
    {
        var statuses = new List<CombatantStatusState>();
        foreach (StatusType type in Enum.GetValues(typeof(StatusType)))
        {
            if (type == StatusType.None)
                continue;

            StatusEffect effect = character[type];
            if (effect == null || !effect.IsApplied)
                continue;

            int duration = 0;
            int magnitude = 0;
            string sourceId = null;

            if (effect is DamageOverTimeStatusEffect damageOverTime)
            {
                duration = damageOverTime.ExpirationTime;
                magnitude = damageOverTime.CurrentTickDamage;
            }
            else if (effect is MarkStatusEffect mark)
            {
                duration = mark.MarkDuration;
            }
            else if (effect is RiposteStatusEffect riposte)
            {
                duration = riposte.RiposteDuration;
            }
            else if (effect is GuardedStatusEffect guarded)
            {
                duration = guarded.GuardDuration;
                if (guarded.Guard != null)
                    sourceId = UnitId(guarded.Guard);
            }
            else if (effect is GuardStatusEffect guard)
            {
                magnitude = guard.Targets.Count;
            }

            statuses.Add(new CombatantStatusState(
                type.ToString(),
                true,
                duration,
                magnitude,
                sourceId));
        }

        return statuses;
    }

    private static IReadOnlyList<CombatantBuffState> ToBuffs(Character character)
    {
        var buffs = new List<CombatantBuffState>();
        foreach (BuffInfo buff in character.GetBuffs())
        {
            if (buff == null || buff.Buff == null)
                continue;

            buffs.Add(new CombatantBuffState(
                buff.Buff.Id ?? string.Empty,
                buff.Buff.AttributeType.ToString(),
                buff.ModifierValue,
                buff.Duration,
                buff.DurationType.ToString(),
                buff.SourceType.ToString(),
                buff.IsApplied));
        }

        return buffs;
    }

    private static AnimationRef CurrentAnimation(FormationUnit unit)
    {
        string animation = CurrentSkeletonAnimation(unit);
        if (string.IsNullOrEmpty(animation))
            animation = CurrentAnimatorClip(unit);
        if (string.IsNullOrEmpty(animation))
        {
            Debug.LogWarning("[GameEventLog] no attack animation for " + unit.Character.Class
                + "; active skeletons: " + ActiveSkeletonNames(unit));
            return null;
        }
        return new AnimationRef(unit.Character.Class, animation);
    }

    private static string CurrentSkeletonAnimation(FormationUnit unit)
    {
        SkeletonAnimation state = unit.CurrentState;
        return state != null ? state.AnimationName : null;
    }

    private static string CurrentAnimatorClip(FormationUnit unit)
    {
        UnitAnimator unitAnimator = unit.GetComponentInChildren<UnitAnimator>();
        Animator animator = unitAnimator != null ? unitAnimator.Animator : null;
        if (animator == null)
            return null;

        AnimatorClipInfo[] clips = animator.IsInTransition(0)
            ? animator.GetNextAnimatorClipInfo(0)
            : animator.GetCurrentAnimatorClipInfo(0);
        if (clips.Length == 0)
            clips = animator.GetCurrentAnimatorClipInfo(0);
        return clips.Length > 0 && clips[0].clip != null ? clips[0].clip.name : null;
    }

    private static string ActiveSkeletonNames(FormationUnit unit)
    {
        var names = new List<string>();
        foreach (SkeletonAnimation skeleton in unit.SkeletonAnimations)
        {
            if (!skeleton.isActiveAndEnabled)
                continue;
            string animation = skeleton.AnimationName;
            names.Add(skeleton.name + "=" + (string.IsNullOrEmpty(animation) ? "<none>" : animation));
        }
        return names.Count > 0 ? string.Join(", ", names.ToArray()) : "<none>";
    }
}
