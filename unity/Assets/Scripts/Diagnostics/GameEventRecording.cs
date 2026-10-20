using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
// Unity 7 injects implicit global usings and its own assemblies contain overlapping type names
// (global StateSnapshot, VectorGraphics.SceneNode, ...); aliases pin every contract type used
// here to the recording DTOs instead of the engine ones.
using AnimationRef = Sektor.DarkestDungeon.Core.GameEvents.AnimationRef;
using AttackExecuted = Sektor.DarkestDungeon.Core.GameEvents.AttackExecuted;
using CombatantState = Sektor.DarkestDungeon.Core.GameEvents.CombatantState;
using GameInput = Sektor.DarkestDungeon.Core.GameEvents.GameInput;
using GameInputKinds = Sektor.DarkestDungeon.Core.GameEvents.GameInputKinds;
using HeroStatChanged = Sektor.DarkestDungeon.Core.GameEvents.HeroStatChanged;
using PartyMoved = Sektor.DarkestDungeon.Core.GameEvents.PartyMoved;
using RoomDiscovered = Sektor.DarkestDungeon.Core.GameEvents.RoomDiscovered;
using SceneGlobals = Sektor.DarkestDungeon.Core.GameEvents.SceneGlobals;
using SceneNode = Sektor.DarkestDungeon.Core.GameEvents.SceneNode;
// SceneSnapshot is deliberately NOT aliased: the diagnostics namespace owns a global
// SceneSnapshot (the scene-dumper DTO); the one event usage below is fully qualified.
using StateSnapshot = Sektor.DarkestDungeon.Core.GameEvents.StateSnapshot;
using StateSnapshotContexts = Sektor.DarkestDungeon.Core.GameEvents.StateSnapshotContexts;
using SubsystemNames = Sektor.DarkestDungeon.Core.GameEvents.SubsystemNames;
using TorchChanged = Sektor.DarkestDungeon.Core.GameEvents.TorchChanged;

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
    private const string PartyRootId = "party_root";
    private const string PartyFormationComponent = "PartyFormation";
    private const string HeroViewComponent = "HeroView";
    private const string SkeletonAnimationComponent = "SkeletonAnimation";

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
        GameEventRecorder recorder = ActiveRecorder();
        if (recorder == null)
            return;

        IList<FormationUnit> heroUnits = RaidSceneManager.BattleGround.HeroParty.Units;
        IList<FormationUnit> monsterUnits = RaidSceneManager.BattleGround.MonsterParty.Units;
        var combatants = new List<CombatantState>(heroUnits.Count + monsterUnits.Count);
        for (int i = 0; i < heroUnits.Count; i++)
            combatants.Add(ToCombatant(heroUnits[i]));
        for (int i = 0; i < monsterUnits.Count; i++)
            combatants.Add(ToCombatant(monsterUnits[i]));

        recorder.Publish(new StateSnapshot(
            StateSnapshotContexts.BattleStart,
            CurrentGlobals(),
            combatants,
            SubsystemNames.Combat));
    }

    /// <summary>
    /// Publishes an hp stat change of a skill target when the executed skill actually moved its
    /// health (miss/dodge publish nothing); called right after BattleSolver.ExecuteSkill.
    /// </summary>
    /// <param name="target">The skill target unit.</param>
    /// <param name="healthBefore">The target's hp captured before the skill executed.</param>
    /// <param name="entry">The result entry of this target.</param>
    public static void PublishSkillResult(FormationUnit target, float healthBefore, SkillResultEntry entry)
    {
        if (target == null || entry == null)
            return;

        int oldHp = Mathf.RoundToInt(healthBefore);
        int newHp = Mathf.RoundToInt(target.Character.CurrentHealth);
        if (oldHp == newHp)
            return;

        GameEventRecorder recorder = ActiveRecorder();
        if (recorder == null)
            return;

        recorder.Publish(new HeroStatChanged(
            UnitId(target), HpStat, oldHp, newHp, entry.Type.ToString(), HpInvariant));
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
        return new CombatantState(
            UnitId(unit),
            unit.Character.Class,
            Mathf.RoundToInt(unit.Character.CurrentHealth),
            Mathf.RoundToInt(unit.Character.MaxHealth),
            unit.Character.IsMonster,
            unit.Character.Class + PortraitSuffix,
            unit.Character.Class);
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
