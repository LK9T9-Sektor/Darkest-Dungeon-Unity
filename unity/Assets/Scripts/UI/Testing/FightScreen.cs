using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Duel.Fight;
using Sektor.DarkestDungeon.Core.Ui;

/// <summary>
/// Fight tester configuration overlay: two sides with four slots each, slot pickers cycling through
/// the available heroes and monsters, a deterministic seed stepper, an AI-mode toggle and the FIGHT
/// button that hands the compositions over to the pure core fight runner.
/// </summary>
public class FightScreen : MonoBehaviour
{
    private const int _slotsPerSide = 4;
    private const int _sortingOrder = 16000;
    private const int _seedMin = 0;
    private const int _seedMax = 9999;

    private static FightScreen _instance;

    private readonly List<string> _candidates = new List<string>();
    private readonly List<int> _slotIndex = new List<int>();
    private readonly Text[] _slotLabels = new Text[_slotsPerSide * 2];
    private readonly List<string> _heroIds = new List<string>();

    private GameObject _panel;
    private Text _seedLabel;
    private Text _modeLabel;
    private Image _aiVsAiBackground;
    private Image _playerVsAiBackground;
    private int _seed = 7;
    private bool _aiVsAi = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (_instance == null)
        {
            GameObject overlayObject = new GameObject(nameof(FightScreen));
            DontDestroyOnLoad(overlayObject);
            overlayObject.AddComponent<FightScreen>();
        }
    }

    /// <summary>Gets a value indicating whether the fight tester overlay exists.</summary>
    public static bool IsAvailable
    {
        get { return _instance != null; }
    }

    /// <summary>Shows the fight tester configuration overlay.</summary>
    public static void Show()
    {
        if (_instance != null)
            _instance._panel.SetActive(true);
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

    private void CreateUi()
    {
        RuntimeUiFactory.EnsureEventSystem();
        Canvas canvas = RuntimeUiFactory.CreateCanvas("FightScreenCanvas", transform, _sortingOrder);
        _panel = CreatePanel(canvas.transform);
        _panel.SetActive(false);
    }

    private GameObject CreatePanel(Transform parent)
    {
        GameObject panel = RuntimeUiFactory.CreateUiObject("FightTesterPanel", parent);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(1520, 960);

        Image background = panel.AddComponent<Image>();
        background.color = RuntimeUiFactory.ToColor(UiStyle.PanelBackground);

        BuildCandidates();

        RuntimeUiFactory.CreateText("FightTesterTitle", panel.transform, "FIGHT TESTER",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -24), new Vector2(600, 44),
            UiStyle.Title, UiStyle.Label, TextAnchor.MiddleLeft);
        CreateTextButton(panel.transform, "CloseButton", "CLOSE", new Vector2(1422, -38),
            new Vector2(110, 44), () => _panel.SetActive(false), new Vector2(0, 1));

        CreateSideLabel(panel.transform, 0, "SIDE #1", new Vector2(40, -84));
        CreateSideLabel(panel.transform, 1, "SIDE #2", new Vector2(790, -84));
        for (int side = 0; side < 2; side++)
            for (int slot = 0; slot < _slotsPerSide; slot++)
                CreateSlotRow(panel.transform, side, slot);

        CreateSeedRow(panel.transform);
        CreateModeRow(panel.transform);

        CreateTextButton(panel.transform, "FightButton", "FIGHT", new Vector2(560, -840),
            new Vector2(400, 56), LaunchFight, new Vector2(0, 1));

        return panel;
    }

    private void BuildCandidates()
    {
        _heroIds.Clear();
        _candidates.Clear();
        _candidates.Add(string.Empty);

        HeroCatalog heroes = FightContentLoader.Heroes;
        if (heroes != null)
            foreach (string id in heroes.ClassIds)
            {
                _heroIds.Add(id);
                _candidates.Add(id);
            }

        MonsterCatalog monsters = FightContentLoader.Monsters;
        if (monsters != null)
            foreach (string id in monsters.Ids)
                _candidates.Add(id);

        _slotIndex.Clear();
        for (int i = 0; i < _slotsPerSide * 2; i++)
            _slotIndex.Add(0);
    }

    private void CreateSideLabel(Transform parent, int side, string text, Vector2 position)
    {
        RuntimeUiFactory.CreateText("SideLabel" + side, parent, text,
            new Vector2(0, 1), new Vector2(0, 1), position, new Vector2(600, 32),
            UiStyle.Body, UiStyle.Label, TextAnchor.MiddleLeft);
    }

    private void CreateSlotRow(Transform parent, int side, int slot)
    {
        int index = side * _slotsPerSide + slot;
        float rowY = -130 - slot * 56;
        float columnX = side == 0 ? 40 : 790;

        CreateTextButton(parent, "SlotLeft" + index, "<", new Vector2(columnX, rowY),
            new Vector2(56, 40), () => ChangeSlot(index, -1), new Vector2(0, 1));

        Text label = RuntimeUiFactory.CreateText("SlotLabel" + index, parent, SlotLabelText(index),
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(columnX + 68, rowY), new Vector2(480, 40),
            UiStyle.Body, UiStyle.Label, TextAnchor.MiddleCenter);
        _slotLabels[index] = label;

        CreateTextButton(parent, "SlotRight" + index, ">", new Vector2(columnX + 580, rowY),
            new Vector2(56, 40), () => ChangeSlot(index, 1), new Vector2(0, 1));
    }

    private void CreateSeedRow(Transform parent)
    {
        float y = -404;
        RuntimeUiFactory.CreateText("SeedTitle", parent, "SEED",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(560, y), new Vector2(200, 40),
            UiStyle.Body, UiStyle.Label, TextAnchor.MiddleLeft);

        CreateTextButton(parent, "SeedDown", "-", new Vector2(760, y), new Vector2(56, 48),
            () => ChangeSeed(-1), new Vector2(0, 1));
        _seedLabel = RuntimeUiFactory.CreateText("SeedValue", parent, _seed.ToString(),
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(826, y), new Vector2(120, 48),
            UiStyle.Body, UiStyle.Label, TextAnchor.MiddleCenter);
        CreateTextButton(parent, "SeedUp", "+", new Vector2(956, y), new Vector2(56, 48),
            () => ChangeSeed(1), new Vector2(0, 1));
    }

    private void CreateModeRow(Transform parent)
    {
        float y = -520;
        RuntimeUiFactory.CreateText("ModeTitle", parent, "MODE",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(560, y), new Vector2(200, 40),
            UiStyle.Body, UiStyle.Label, TextAnchor.MiddleLeft);

        _aiVsAiBackground = CreateTextButton(parent, "AiVsAiButton", "AI vs AI", new Vector2(760, y),
            new Vector2(220, 48), () => SetMode(true), new Vector2(0, 1));
        _playerVsAiBackground = CreateTextButton(parent, "PlayerVsAiButton", "PLAYER / AI", new Vector2(1020, y),
            new Vector2(220, 48), () => SetMode(false), new Vector2(0, 1));

        _modeLabel = RuntimeUiFactory.CreateText("ModeHint", parent, string.Empty,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(560, y - 56), new Vector2(680, 32),
            UiStyle.Small, UiStyle.Label, TextAnchor.MiddleLeft);

        SetMode(true);
    }

    private void SetMode(bool aiVsAi)
    {
        _aiVsAi = aiVsAi;
        SetButtonTint(_aiVsAiBackground, _aiVsAi);
        SetButtonTint(_playerVsAiBackground, !_aiVsAi);
        _modeLabel.text = _aiVsAi
            ? "Both sides act automatically."
            : "SIDE #1 heroes act manually: pick a skill, then a target.";
    }

    private static void SetButtonTint(Image background, bool selected)
    {
        if (background != null)
            background.color = RuntimeUiFactory.ToColor(selected ? UiStyle.SelectedRow : UiStyle.ButtonBackground);
    }

    private void ChangeSlot(int index, int delta)
    {
        int count = _candidates.Count;
        if (count == 0)
            return;

        _slotIndex[index] = (_slotIndex[index] + delta + count) % count;
        _slotLabels[index].text = SlotLabelText(index);
    }

    private string SlotLabelText(int index)
    {
        int candidateIndex = _slotIndex[index];
        if (candidateIndex == 0)
            return "- empty -";

        string id = _candidates[candidateIndex];
        return candidateIndex <= _heroIds.Count ? "Hero: " + id : "Monster: " + id;
    }

    private void ChangeSeed(int delta)
    {
        _seed = Mathf.Clamp(_seed + delta, _seedMin, _seedMax);
        _seedLabel.text = _seed.ToString();
    }

    private void LaunchFight()
    {
        var side1 = new List<FightUnitSpec>();
        var side2 = new List<FightUnitSpec>();
        for (int side = 0; side < 2; side++)
        {
            List<FightUnitSpec> sideList = side == 0 ? side1 : side2;
            for (int slot = 0; slot < _slotsPerSide; slot++)
            {
                int index = side * _slotsPerSide + slot;
                int candidateIndex = _slotIndex[index];
                if (candidateIndex == 0)
                    continue;

                string id = _candidates[candidateIndex];
                if (candidateIndex <= _heroIds.Count)
                {
                    Sektor.DarkestDungeon.Core.Combat.Character.HeroClass heroClass = FightContentLoader.Content.GetHeroClass(id);
                    if (heroClass == null)
                        continue;

                    var skillIds = heroClass.CombatSkills.Take(4).Select(skill => skill.Id).ToList();
                    sideList.Add(new HeroFightUnitSpec(id, _seed + index + 1, skillIds, null));
                }
                else
                {
                    sideList.Add(new MonsterFightUnitSpec(id));
                }
            }
        }

        if (side1.Count == 0 || side2.Count == 0)
            return;

        if (FightBattleView.Instance != null)
            FightBattleView.Instance.Begin(side1, side2, _seed, _aiVsAi);
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
            UiStyle.Body, UiStyle.Label, TextAnchor.MiddleCenter);
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.sizeDelta = Vector2.zero;

        return background;
    }
}