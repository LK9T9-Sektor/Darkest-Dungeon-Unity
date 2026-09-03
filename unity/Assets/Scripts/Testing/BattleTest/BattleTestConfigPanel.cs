using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Ui;

/// <summary>
/// The battle test setup overlay: two sides with four slots each (hero or monster class pickers), the
/// control mode (player/AI, AI vs AI, hotseat), the seed and torch steppers and the FIGHT button that
/// hands the composition over to <see cref="CoreBattleDriver"/>.
/// </summary>
public class BattleTestConfigPanel : MonoBehaviour
{
    private const int SlotsPerSide = 4;
    private const int SortingOrder = 16000;
    private const int SeedMin = 0;
    private const int SeedMax = 9999;
    private const int TorchMin = 0;
    private const int TorchMax = 100;

    [SerializeField]
    private CoreBattleDriver driver;

    private readonly List<string> _candidates = new List<string>();
    private readonly List<int> _slotIndex = new List<int>();
    private readonly List<string> _heroIds = new List<string>();
    private readonly Text[] _slotLabels = new Text[SlotsPerSide * 2];

    private GameObject _panel;
    private Text _seedLabel;
    private Text _torchLabel;
    private Image _playerAiBackground;
    private Image _aiAiBackground;
    private Image _hotseatBackground;
    private int _seed = 7;
    private int _torch = 75;
    private int _mode;

    private void Start()
    {
        CreateUi();
        Show();
    }

    private void CreateUi()
    {
        RuntimeUiFactory.EnsureEventSystem();
        Canvas canvas = RuntimeUiFactory.CreateCanvas("BattleTestConfigCanvas", transform, SortingOrder);
        _panel = CreatePanel(canvas.transform);
        _panel.SetActive(false);
    }

    /// <summary>Shows the setup overlay.</summary>
    public void Show()
    {
        if (_panel != null)
            _panel.SetActive(true);
    }

    private GameObject CreatePanel(Transform parent)
    {
        GameObject panel = RuntimeUiFactory.CreateUiObject("BattleTestConfigPanel", parent);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(1520, 960);

        Image background = panel.AddComponent<Image>();
        background.color = RuntimeUiFactory.ToColor(UiStyle.PanelBackground);

        BuildCandidates();

        RuntimeUiFactory.CreateText("Title", panel.transform, "BATTLE TEST",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -24), new Vector2(600, 44),
            UiStyle.Title, UiStyle.Label, TextAnchor.MiddleLeft);
        CreateButton(panel.transform, "CloseButton", "CLOSE", new Vector2(1422, -38),
            new Vector2(110, 44), () => _panel.SetActive(false), new Vector2(0, 1));

        CreateSideLabel(panel.transform, 0, "HEROES", new Vector2(40, -84));
        CreateSideLabel(panel.transform, 1, "OPPONENT", new Vector2(790, -84));
        for (int side = 0; side < 2; side++)
            for (int slot = 0; slot < SlotsPerSide; slot++)
                CreateSlotRow(panel.transform, side, slot);

        CreateStepperRow(panel.transform, "SEED", "SeedValue", -420, ref _seedLabel, ChangeSeed);
        CreateStepperRow(panel.transform, "TORCH", "TorchValue", -480, ref _torchLabel, ChangeTorch);
        CreateModeRow(panel.transform);

        CreateButton(panel.transform, "FightButton", "FIGHT", new Vector2(560, -860),
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
        for (int i = 0; i < SlotsPerSide * 2; i++)
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
        int index = side * SlotsPerSide + slot;
        float rowY = -130 - slot * 56;
        float columnX = side == 0 ? 40 : 790;

        CreateButton(parent, "SlotLeft" + index, "<", new Vector2(columnX, rowY),
            new Vector2(56, 40), () => ChangeSlot(index, -1), new Vector2(0, 1));

        Text label = RuntimeUiFactory.CreateText("SlotLabel" + index, parent, SlotLabelText(index),
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(columnX + 68, rowY), new Vector2(480, 40),
            UiStyle.Body, UiStyle.Label, TextAnchor.MiddleCenter);
        _slotLabels[index] = label;

        CreateButton(parent, "SlotRight" + index, ">", new Vector2(columnX + 580, rowY),
            new Vector2(56, 40), () => ChangeSlot(index, 1), new Vector2(0, 1));
    }

    private void CreateStepperRow(Transform parent, string title, string valueName, float y,
        ref Text valueLabel, UnityAction<int> change)
    {
        RuntimeUiFactory.CreateText(title, parent, title,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(560, y), new Vector2(200, 40),
            UiStyle.Body, UiStyle.Label, TextAnchor.MiddleLeft);

        CreateButton(parent, title + "Down", "-", new Vector2(760, y), new Vector2(56, 48),
            () => change(-1), new Vector2(0, 1));
        valueLabel = RuntimeUiFactory.CreateText(valueName, parent, "0",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(826, y), new Vector2(120, 48),
            UiStyle.Body, UiStyle.Label, TextAnchor.MiddleCenter);
        CreateButton(parent, title + "Up", "+", new Vector2(956, y), new Vector2(56, 48),
            () => change(1), new Vector2(0, 1));
    }

    private void CreateModeRow(Transform parent)
    {
        float y = -560;
        RuntimeUiFactory.CreateText("ModeTitle", parent, "MODE",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(560, y), new Vector2(200, 40),
            UiStyle.Body, UiStyle.Label, TextAnchor.MiddleLeft);

        _playerAiBackground = CreateButton(parent, "PlayerAiButton", "PLAYER / AI", new Vector2(760, y),
            new Vector2(200, 48), () => SetMode(0), new Vector2(0, 1));
        _aiAiBackground = CreateButton(parent, "AiAiButton", "AI vs AI", new Vector2(990, y),
            new Vector2(180, 48), () => SetMode(1), new Vector2(0, 1));
        _hotseatBackground = CreateButton(parent, "HotseatButton", "HOTSEAT", new Vector2(1200, y),
            new Vector2(180, 48), () => SetMode(2), new Vector2(0, 1));

        SetMode(0);
    }

    private void SetMode(int mode)
    {
        _mode = mode;
        SetButtonTint(_playerAiBackground, mode == 0);
        SetButtonTint(_aiAiBackground, mode == 1);
        SetButtonTint(_hotseatBackground, mode == 2);
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
        _seed = Mathf.Clamp(_seed + delta, SeedMin, SeedMax);
        _seedLabel.text = _seed.ToString();
    }

    private void ChangeTorch(int delta)
    {
        _torch = Mathf.Clamp(_torch + delta, TorchMin, TorchMax);
        _torchLabel.text = _torch.ToString();
    }

    private void LaunchFight()
    {
        if (driver == null)
            return;

        var config = new BattleTestConfig { Seed = _seed, Torch = _torch };
        for (int side = 0; side < 2; side++)
        {
            List<BattleTestSlotSpec> slots = side == 0 ? config.Side1.Slots : config.Side2.Slots;
            for (int slot = 0; slot < SlotsPerSide; slot++)
            {
                int index = side * SlotsPerSide + slot;
                int candidateIndex = _slotIndex[index];
                if (candidateIndex == 0)
                    continue;

                string id = _candidates[candidateIndex];
                var spec = new BattleTestSlotSpec { ClassId = id, IsHero = candidateIndex <= _heroIds.Count };
                slots.Add(spec);
            }
        }

        config.PlayerControlsSide1 = _mode != 1;
        config.PlayerControlsSide2 = _mode == 2;

        if (config.Side1.Slots.Count == 0 || config.Side2.Slots.Count == 0)
            return;

        _panel.SetActive(false);
        driver.Begin(config);
    }

    private Image CreateButton(Transform parent, string name, string label, Vector2 position,
        Vector2 size, UnityAction onClick, Vector2 anchor)
    {
        GameObject buttonObject = RuntimeUiFactory.CreateUiObject(name, parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image background = buttonObject.AddComponent<Image>();
        background.color = RuntimeUiFactory.ToColor(UiStyle.ButtonBackground);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(onClick);

        Text labelText = RuntimeUiFactory.CreateText("Label", buttonObject.transform, label,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero,
            UiStyle.Body, UiStyle.Label, TextAnchor.MiddleCenter);
        labelText.rectTransform.anchorMin = Vector2.zero;
        labelText.rectTransform.anchorMax = Vector2.one;
        labelText.rectTransform.sizeDelta = Vector2.zero;

        return background;
    }
}