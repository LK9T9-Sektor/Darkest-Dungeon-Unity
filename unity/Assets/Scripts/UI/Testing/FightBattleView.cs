using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Duel;
using Sektor.DarkestDungeon.Core.Duel.Fight;
using Sektor.DarkestDungeon.Core.Ui;

/// <summary>
/// Fight battle view driven by the pure core fight runner: unit cards (heroes left, monsters right),
/// a rolling action log, round/actor header and, in manual mode, skill + target selection for the
/// player-controlled heroes.
/// </summary>
public class FightBattleView : MonoBehaviour
{
    private const int _sortingOrder = 17000;
    private const int _maxTicksPerFrame = 12;
    private const int _maxLogLines = 140;

    private static FightBattleView _instance;

    private static readonly ArgbColor ActorTint = new ArgbColor(242, 115, 97, 51);
    private static readonly ArgbColor TargetTint = new ArgbColor(242, 40, 90, 40);
    private static readonly ArgbColor DeadTint = new ArgbColor(230, 30, 30, 30);

    private GameObject _panel;
    private Text _roundLabel;
    private Text _actorLabel;
    private Text _resultLabel;
    private Text _logText;
    private Text _skillHint;
    private RectTransform _fieldRoot;
    private RectTransform _skillRoot;
    private readonly List<string> _logLines = new List<string>();
    private readonly Dictionary<int, string> _lastActions = new Dictionary<int, string>();

    private FightSession _session;
    private bool _aiVsAi;
    private bool _forcedAuto;
    private string _pendingSkillId;
    private string _viewHash = string.Empty;
    private bool _finished;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (_instance == null)
        {
            GameObject overlayObject = new GameObject(nameof(FightBattleView));
            DontDestroyOnLoad(overlayObject);
            overlayObject.AddComponent<FightBattleView>();
        }
    }

    /// <summary>Gets the overlay instance (created at load).</summary>
    public static FightBattleView Instance
    {
        get { return _instance; }
    }

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        CreateUi();
    }

    /// <summary>Starts a new fight with the given compositions and shows the battle view.</summary>
    public void Begin(List<FightUnitSpec> side1, List<FightUnitSpec> side2, int seed, bool aiVsAi)
    {
        _aiVsAi = aiVsAi;
        _forcedAuto = false;
        _pendingSkillId = null;
        _logLines.Clear();
        _lastActions.Clear();
        _finished = false;
        _viewHash = string.Empty;
        _resultLabel.text = string.Empty;
        _logText.text = string.Empty;

        _session = new FightSession(FightContentLoader.Content, seed);
        _session.Start(side1, side2);

        _panel.SetActive(true);
        Refresh();
    }

    /// <summary>Closes the battle view and returns to the fight tester configuration.</summary>
    public void Close()
    {
        _session = null;
        _pendingSkillId = null;
        _panel.SetActive(false);
        if (FightScreen.IsAvailable)
            FightScreen.Show();
    }

    private void Update()
    {
        if (_session == null || !_session.IsStarted)
            return;

        if (_session.IsFinished)
        {
            if (!_finished)
            {
                _finished = true;
                _resultLabel.text = "WINNER: " + WinnerName();
                AppendLog("--- " + _resultLabel.text + " ---");
                Refresh();
            }
            return;
        }

        if (_aiVsAi || _forcedAuto)
        {
            AdvanceTicks(_maxTicksPerFrame);
        }
        else if (!_session.IsWaitingForPlayerAction)
        {
            AdvanceTicks(_maxTicksPerFrame);
        }

        Refresh();
    }

    private void CreateUi()
    {
        RuntimeUiFactory.EnsureEventSystem();
        Canvas canvas = RuntimeUiFactory.CreateCanvas("FightBattleCanvas", transform, _sortingOrder);
        _panel = CreatePanel(canvas.transform);
        _panel.SetActive(false);
    }

    private GameObject CreatePanel(Transform parent)
    {
        GameObject panel = RuntimeUiFactory.CreateUiObject("FightBattlePanel", parent);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(1520, 960);

        Image background = panel.AddComponent<Image>();
        background.color = RuntimeUiFactory.ToColor(UiStyle.PanelBackground);

        _roundLabel = RuntimeUiFactory.CreateText("RoundLabel", panel.transform, string.Empty,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -24), new Vector2(300, 40),
            UiStyle.Body, UiStyle.Label, TextAnchor.MiddleLeft);
        _actorLabel = RuntimeUiFactory.CreateText("ActorLabel", panel.transform, string.Empty,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(360, -24), new Vector2(620, 40),
            UiStyle.Body, UiStyle.Label, TextAnchor.MiddleCenter);
        _resultLabel = RuntimeUiFactory.CreateText("ResultLabel", panel.transform, string.Empty,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(1000, -24), new Vector2(360, 40),
            UiStyle.Body, UiStyle.Label, TextAnchor.MiddleCenter);

        CreateTextButton(panel.transform, "AutoToggleButton", "AUTO", new Vector2(1160, -44),
            new Vector2(140, 44), ToggleAuto, new Vector2(0, 1));
        CreateTextButton(panel.transform, "BattleCloseButton", "X", new Vector2(1422, -38),
            new Vector2(56, 44), Close, new Vector2(0, 1));

        _fieldRoot = AnchorTopLeft(RuntimeUiFactory.CreateUiObject("FieldRoot", panel.transform));
        _skillRoot = AnchorTopLeft(RuntimeUiFactory.CreateUiObject("SkillRoot", panel.transform));

        _skillHint = RuntimeUiFactory.CreateText("SkillHint", panel.transform, string.Empty,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -580), new Vector2(760, 32),
            UiStyle.Small, UiStyle.Label, TextAnchor.MiddleLeft);

        GameObject logRoot = RuntimeUiFactory.CreateUiObject("LogRoot", panel.transform);
        RectTransform logRect = logRoot.GetComponent<RectTransform>();
        logRect.anchorMin = new Vector2(0, 1);
        logRect.anchorMax = new Vector2(0, 1);
        logRect.pivot = new Vector2(0, 1);
        logRect.anchoredPosition = new Vector2(40, -640);
        logRect.sizeDelta = new Vector2(1440, 200);
        Image logBackground = logRoot.AddComponent<Image>();
        logBackground.color = new Color(0, 0, 0, 0.4f);

        _logText = RuntimeUiFactory.CreateText("LogText", logRoot.transform, string.Empty,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(16, -8), new Vector2(1408, 150),
            UiStyle.LogBody, UiStyle.Label, TextAnchor.UpperLeft);
        _logText.horizontalOverflow = HorizontalWrapMode.Wrap;
        _logText.verticalOverflow = VerticalWrapMode.Overflow;

        return panel;
    }

    private static RectTransform AnchorTopLeft(GameObject uiObject)
    {
        RectTransform rect = uiObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        return rect;
    }

    private void ToggleAuto()
    {
        _forcedAuto = !_forcedAuto;
        if (!_forcedAuto)
            _pendingSkillId = null;
        Refresh();
    }

    private void AdvanceTicks(int maxTicks)
    {
        int ticks = 0;
        while (ticks++ < maxTicks && _session.Tick())
        {
        }
        FlushActionLog();
    }

    private string WinnerName()
    {
        bool heroesAlive = _session.Duel.HeroParty.Units.Any(unit => !unit.CombatInfo.IsDead);
        bool monstersAlive = _session.Duel.MonsterParty.Units.Any(unit => !unit.CombatInfo.IsDead);
        if (heroesAlive == monstersAlive)
            return "DRAW";
        return heroesAlive ? "HEROES" : "MONSTERS";
    }

    private void FlushActionLog()
    {
        foreach (ICombatUnit unit in AllUnits())
        {
            if (unit.CombatInfo == null)
                continue;

            string played = unit.CombatInfo.LastCombatSkillUsed + ">" + unit.CombatInfo.LastCombatSkillTarget;
            string previous;
            if (_lastActions.TryGetValue(unit.CombatInfo.CombatId, out previous) && previous == played)
                continue;

            _lastActions[unit.CombatInfo.CombatId] = played;
            if (string.IsNullOrEmpty(unit.CombatInfo.LastCombatSkillUsed))
                continue;

            ICombatUnit target = _session.Duel.GetUnitByCombatId(unit.CombatInfo.LastCombatSkillTarget);
            string targetName = target != null ? UnitsName(target) : "?";
            AppendLog("[" + _session.Duel.BattleGround.Round.RoundNumber + "] " +
                UnitsName(unit) + " used " + unit.CombatInfo.LastCombatSkillUsed + " on " + targetName);
        }
    }

    private void AppendLog(string line)
    {
        _logLines.Add(line);
        while (_logLines.Count > _maxLogLines)
            _logLines.RemoveAt(0);
        _logText.text = string.Join("\n", _logLines.ToArray());
    }

    private void Refresh()
    {
        if (_session == null || !_session.IsStarted)
        {
            _panel.SetActive(false);
            return;
        }

        string hash = BuildViewHash();
        if (hash == _viewHash)
            return;

        _viewHash = hash;
        RebuildField();
        RebuildSkills();
        FlushActionLog();
    }

    private string BuildViewHash()
    {
        var builder = new StringBuilder();
        builder.Append(_aiVsAi).Append('|').Append(_forcedAuto).Append('|').Append(_pendingSkillId ?? "-").Append('|');
        builder.Append(_session.Duel.Phase).Append('|');
        builder.Append(_session.Duel.BattleGround.Round.RoundNumber).Append('|');
        ICombatUnit current = _session.Duel.CurrentUnit;
        builder.Append(current != null ? current.CombatInfo.CombatId : 0).Append('|');
        foreach (ICombatUnit unit in AllUnits())
        {
            builder.Append(unit.CombatInfo.CombatId).Append(':')
                .Append(unit.CombatInfo.IsDead).Append(':')
                .Append(unit.Character.HealthRatio).Append(';');
        }
        return builder.ToString();
    }

    private void RebuildField()
    {
        for (int i = 0; i < _fieldRoot.childCount; i++)
            Destroy(_fieldRoot.GetChild(i).gameObject);

        List<ICombatUnit> heroes = OrderedHeroes();
        List<ICombatUnit> monsters = OrderedMonsters();

        ICombatUnit current = _session.Duel.CurrentUnit;
        List<int> legalTargets = LegalTargetIds(current);

        for (int i = 0; i < heroes.Count; i++)
            BuildCard(_fieldRoot, heroes[i], new Vector2(20, 16 + i * 106), i,
                current, legalTargets);
        for (int i = 0; i < monsters.Count; i++)
            BuildCard(_fieldRoot, monsters[i], new Vector2(1220, 16 + i * 106), i,
                current, legalTargets);

        _roundLabel.text = "ROUND " + _session.Duel.BattleGround.Round.RoundNumber;
        _actorLabel.text = current != null ? "acting: " + UnitsName(current) : string.Empty;
    }

    private void BuildCard(Transform root, ICombatUnit unit, Vector2 position, int index,
        ICombatUnit current, List<int> legalTargets)
    {
        GameObject card = RuntimeUiFactory.CreateUiObject("Card_" + unit.CombatInfo.CombatId, root);
        RectTransform rect = card.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(280, 94);

        Image background = card.AddComponent<Image>();
        background.color = RuntimeUiFactory.ToColor(unit.CombatInfo.IsDead ? DeadTint : UiStyle.IdleRow);
        if (!unit.CombatInfo.IsDead)
        {
            if (current != null && current.CombatInfo.CombatId == unit.CombatInfo.CombatId)
                background.color = RuntimeUiFactory.ToColor(ActorTint);
            else if (legalTargets.Contains(unit.CombatInfo.CombatId))
                background.color = RuntimeUiFactory.ToColor(TargetTint);
        }

        Button button = card.AddComponent<Button>();
        button.targetGraphic = background;
        int targetId = unit.CombatInfo.CombatId;
        button.onClick.AddListener(() => SelectTarget(targetId));

        RuntimeUiFactory.CreateText("Name", card.transform, UnitsName(unit),
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, -6), new Vector2(260, 28),
            UiStyle.Body, UiStyle.Label, TextAnchor.MiddleLeft);

        string health = "HP " + Mathf.RoundToInt(unit.Character.HealthRatio * 100f) + "%";
        string stress = unit.Character.IsMonster ? string.Empty : "  stress " + StressText(unit);
        string line = unit.CombatInfo.IsDead ? "DEAD" : health + stress;
        RuntimeUiFactory.CreateText("Health", card.transform, line,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, -42), new Vector2(260, 28),
            UiStyle.Small, UiStyle.Label, TextAnchor.MiddleLeft);
    }

    private static string StressText(ICombatUnit unit)
    {
        if (unit.Character.Stress == null)
            return "0";
        return Mathf.RoundToInt(unit.Character.Stress.CurrentValue).ToString();
    }

    private void RebuildSkills()
    {
        for (int i = 0; i < _skillRoot.childCount; i++)
            Destroy(_skillRoot.GetChild(i).gameObject);

        _skillHint.text = string.Empty;
        if (_session == null || !_session.IsStarted)
            return;

        if (_aiVsAi || _forcedAuto)
        {
            _skillHint.text = "AI RUNNING...";
            return;
        }

        if (!_session.IsWaitingForPlayerAction)
        {
            _skillHint.text = "AUTO PROGRESS...";
            return;
        }

        ICombatUnit unit = _session.Duel.CurrentUnit;
        _skillHint.text = "YOUR TURN: " + UnitsName(unit) + " — pick a skill, then a target.";

        if (unit.Character.CurrentCombatSkills != null)
        {
            int column = 0;
            foreach (Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.CombatSkill skill in unit.Character.CurrentCombatSkills)
            {
                if (skill == null)
                    continue;

                string skillId = skill.Id;
                CreateTextButton(_skillRoot, "Skill_" + column, skillId,
                    new Vector2(500 + column * 150, -560), new Vector2(140, 44),
                    () => SelectSkill(skillId), new Vector2(0, 1));
                column++;
            }
        }

        CreateTextButton(_skillRoot, "Pass_Button", "PASS", new Vector2(1220, -560),
            new Vector2(140, 44), PassTurn, new Vector2(0, 1));
    }

    private void SelectSkill(string skillId)
    {
        _pendingSkillId = skillId;
        Refresh();
    }

    private void SelectTarget(int targetId)
    {
        if (_pendingSkillId == null)
            return;

        var action = new FightPlayerAction(_pendingSkillId, targetId);
        _session.Tick(action);
        _pendingSkillId = null;

        Refresh();
    }

    private void PassTurn()
    {
        if (_session.Duel.Phase == DuelPhase.WaitingForHostAction && !_session.Duel.IsLocalTurn)
        {
            _session.Duel.ApplyRemoteSkill(DuelPayload.PassAction());
        }
        else if (_session.Duel.IsLocalTurn)
        {
            _session.Duel.ExecuteLocalPass();
        }
        _pendingSkillId = null;
        Refresh();
    }

    private List<int> LegalTargetIds(ICombatUnit current)
    {
        var result = new List<int>();
        if (_pendingSkillId == null || current == null || !_session.IsWaitingForPlayerAction)
            return result;

        Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.CombatSkill skill = null;
        if (current.Character.CurrentCombatSkills != null)
            skill = current.Character.CurrentCombatSkills.FirstOrDefault(candidate => candidate.Id == _pendingSkillId);
        if (skill == null)
            return result;

        foreach (ICombatUnit target in _session.Duel.GetAvailableTargets(current, skill))
            if (target.CombatInfo != null)
                result.Add(target.CombatInfo.CombatId);
        return result;
    }

    private List<ICombatUnit> OrderedHeroes()
    {
        var units = _session.Duel.HeroParty.Units
            .Where(unit => unit.CombatInfo != null)
            .OrderByDescending(unit => unit.CombatInfo.IsDead)
            .ThenBy(unit => unit.Rank)
            .ToList();
        units.Reverse();
        return units;
    }

    private List<ICombatUnit> OrderedMonsters()
    {
        return _session.Duel.MonsterParty.Units
            .Where(unit => unit.CombatInfo != null)
            .OrderByDescending(unit => unit.CombatInfo.IsDead)
            .ThenBy(unit => unit.Rank)
            .ToList();
    }

    private IEnumerable<ICombatUnit> AllUnits()
    {
        var result = new List<ICombatUnit>();
        result.AddRange(_session.Duel.HeroParty.Units);
        result.AddRange(_session.Duel.MonsterParty.Units);
        return result;
    }

    private static string UnitsName(ICombatUnit unit)
    {
        if (unit == null || unit.Character == null)
            return "?";
        return unit.Character.Name + " #" + unit.CombatInfo.CombatId;
    }

    private Image CreateTextButton(Transform parent, string name, string label,
        Vector2 position, Vector2 size, UnityAction onClick, Vector2 anchor)
    {
        GameObject buttonObject = RuntimeUiFactory.CreateUiObject(name, parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;

        Image background = buttonObject.AddComponent<Image>();
        background.color = RuntimeUiFactory.ToColor(UiStyle.ButtonBackground);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(onClick);

        Text text = RuntimeUiFactory.CreateText("Label", buttonObject.transform, label,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero,
            UiStyle.Small, UiStyle.Label, TextAnchor.MiddleCenter);
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.sizeDelta = Vector2.zero;

        return background;
    }
}