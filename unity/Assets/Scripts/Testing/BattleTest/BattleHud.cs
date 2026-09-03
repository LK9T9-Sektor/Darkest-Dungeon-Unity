using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Ui;

/// <summary>
/// The battle screen overlay: round indicator, announcements, the acting unit's skill buttons with
/// target selection, a pass button, the battle log and the winner banner. Pure view; all actions are
/// forwarded to <see cref="CoreBattleDriver"/>.
/// </summary>
public class BattleHud : MonoBehaviour
{
    private const int SortingOrder = 12000;
    private const int SkillFontSize = 18;

    private readonly List<GameObject> _skillButtons = new List<GameObject>();
    private readonly List<GameObject> _targetButtons = new List<GameObject>();
    private readonly List<string> _logLines = new List<string>();

    private CoreBattleDriver _driver;
    private Text _roundText;
    private Text _announcement;
    private Text _winner;
    private Text _log;
    private Transform _skillRow;
    private Transform _targetRow;
    private GameObject _passButton;
    private string _selectedSkillId;
    private bool _created;

    /// <summary>Binds the HUD to the driver and creates the overlay canvas.</summary>
    /// <param name="driver">The battle driver.</param>
    public void Bind(CoreBattleDriver driver)
    {
        _driver = driver;
        if (!_created)
            CreateUi();
    }

    /// <summary>Clears transient state for a new battle.</summary>
    public void Clear()
    {
        _logLines.Clear();
        SetRound(0);
        Announce(string.Empty);
        ShowWinner(null);
        HideActor();
    }

    /// <summary>Shows the acting unit's skills and enables the pass button.</summary>
    /// <param name="actor">The acting unit.</param>
    public void ShowActor(ICombatUnit actor)
    {
        ClearSkillButtons();
        ClearTargetButtons();
        _selectedSkillId = null;

        if (actor == null || actor.Character == null)
            return;

        List<Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.CombatSkill> skills = actor.Character.CurrentCombatSkills;
        float x = -((skills.Count - 1) * 70f) / 2f;
        foreach (Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.CombatSkill skill in skills)
        {
            if (skill == null)
                continue;
            string skillId = skill.Id;
            CreateButton(_skillRow, "Skill" + skillId, skillId, new Vector2(x, 0), new Vector2(64, 36),
                () => SelectSkill(skillId));
            x += 70;
        }

        if (_passButton != null)
            _passButton.SetActive(true);
    }

    /// <summary>Hides the acting unit's skills and the pass button.</summary>
    public void HideActor()
    {
        ClearSkillButtons();
        ClearTargetButtons();
        _selectedSkillId = null;
        if (_passButton != null)
            _passButton.SetActive(false);
    }

    /// <summary>Sets the round number text.</summary>
    /// <param name="round">The round number.</param>
    public void SetRound(int round)
    {
        if (_roundText != null)
            _roundText.text = "ROUND " + round;
    }

    /// <summary>Shows a short announcement in the centre.</summary>
    /// <param name="text">The announcement text, or empty to hide.</param>
    public void Announce(string text)
    {
        if (_announcement != null)
            _announcement.text = text;
    }

    /// <summary>Shows the winner banner, or hides it when null.</summary>
    /// <param name="text">The winner text, or null to hide.</param>
    public void ShowWinner(string text)
    {
        if (_winner != null)
            _winner.gameObject.SetActive(text != null);
        if (_winner != null && text != null)
            _winner.text = text;
    }

    /// <summary>Appends a line to the battle log.</summary>
    /// <param name="line">The log line.</param>
    public void AppendLog(string line)
    {
        _logLines.Add(line);
        while (_logLines.Count > 8)
            _logLines.RemoveAt(0);
        if (_log != null)
            _log.text = string.Join("\n", _logLines);
    }

    private void SelectSkill(string skillId)
    {
        if (_driver == null || _driver.Duel == null)
            return;

        ICombatUnit actor = _driver.Duel.CurrentUnit;
        if (actor == null)
            return;

        var skills = actor.Character.CurrentCombatSkills;
        Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.CombatSkill skill = skills.FirstOrDefault(item => item != null && item.Id == skillId);
        if (skill == null)
            return;

        _selectedSkillId = skillId;
        ClearTargetButtons();

        var targets = _driver.Duel.GetAvailableTargets(actor, skill);
        float x = -((targets.Count - 1) * 140f) / 2f;
        foreach (ICombatUnit target in targets)
        {
            int targetId = target.CombatInfo.CombatId;
            string label = target.Character.Name + " (r" + target.Rank + ")";
            CreateButton(_targetRow, "Target" + targetId, label, new Vector2(x, 0), new Vector2(132, 34),
                () => SelectTarget(targetId));
            x += 140;
        }
    }

    private void SelectTarget(int targetCombatId)
    {
        if (_selectedSkillId == null || _driver == null)
            return;
        string skillId = _selectedSkillId;
        _selectedSkillId = null;
        ClearTargetButtons();
        _driver.PlayerAct(skillId, targetCombatId);
    }

    private void PassTurn()
    {
        _selectedSkillId = null;
        ClearTargetButtons();
        if (_driver != null)
            _driver.PlayerPass();
    }

    private void CreateUi()
    {
        _created = true;

        Canvas canvas = RuntimeUiFactory.CreateCanvas("BattleHud", transform, SortingOrder);

        _roundText = RuntimeUiFactory.CreateText("Round", canvas.transform, "ROUND 0",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20), new Vector2(260, 36),
            UiStyle.Title, UiStyle.Label, TextAnchor.MiddleLeft);

        _announcement = RuntimeUiFactory.CreateText("Announcement", canvas.transform, string.Empty,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -120), new Vector2(1200, 60),
            UiStyle.Title, UiStyle.Label, TextAnchor.MiddleCenter);

        _winner = RuntimeUiFactory.CreateText("Winner", canvas.transform, string.Empty,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900, 120), 72);
        _winner.gameObject.SetActive(false);

        _log = RuntimeUiFactory.CreateText("Log", canvas.transform, string.Empty,
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(20, 24), new Vector2(620, 260),
            UiStyle.Small, UiStyle.Label, TextAnchor.LowerLeft);

        _skillRow = RuntimeUiFactory.CreateUiObject("SkillRow", canvas.transform).transform;
        _targetRow = RuntimeUiFactory.CreateUiObject("TargetRow", canvas.transform).transform;

        RectTransform skillRect = _skillRow as RectTransform;
        skillRect.anchorMin = new Vector2(0.5f, 0f);
        skillRect.anchorMax = new Vector2(0.5f, 0f);
        skillRect.pivot = new Vector2(0.5f, 0f);
        skillRect.anchoredPosition = new Vector2(0f, 34f);

        RectTransform targetRect = _targetRow as RectTransform;
        targetRect.anchorMin = new Vector2(0.5f, 0f);
        targetRect.anchorMax = new Vector2(0.5f, 0f);
        targetRect.pivot = new Vector2(0.5f, 0f);
        targetRect.anchoredPosition = new Vector2(0f, 110f);

        _passButton = CreateButton(canvas.transform, "Pass", "PASS", new Vector2(330, 34),
            new Vector2(96, 36), PassTurn).gameObject;
        _passButton.SetActive(false);
    }

    private Button CreateButton(Transform parent, string name, string label, Vector2 position,
        Vector2 size, UnityAction onClick)
    {
        GameObject buttonObject = RuntimeUiFactory.CreateUiObject(name, parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image background = buttonObject.AddComponent<Image>();
        background.color = RuntimeUiFactory.ToColor(UiStyle.ButtonBackground);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(onClick);

        RuntimeUiFactory.CreateText("Label", buttonObject.transform, label,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero,
            SkillFontSize, UiStyle.Label, TextAnchor.MiddleCenter);
        return button;
    }

    private void ClearSkillButtons()
    {
        ClearButtons(_skillButtons);
    }

    private void ClearTargetButtons()
    {
        ClearButtons(_targetButtons);
    }

    private static void ClearButtons(List<GameObject> buttons)
    {
        foreach (GameObject button in buttons)
            Destroy(button);
        buttons.Clear();
    }
}