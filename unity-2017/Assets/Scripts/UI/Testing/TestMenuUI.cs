using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Sektor.DarkestDungeon.Core.Ui;

/// <summary>
/// Runtime-created TEST menu shown on the campaign selection screen. Provides a bottom-right
/// "TEST" button that opens a panel with the <see cref="TestActions"/> checks and a log area,
/// so content/core regressions are verifiable in-game without touching the scene UI.
/// </summary>
public class TestMenuUI : MonoBehaviour
{
    private const string _campaignSelectionSceneName = "CampaignSelection";
    private const string _panelTitleLabel = "TEST MENU";
    private const string _clearButtonLabel = "Clear";
    private const int _sortingOrder = 15000;
    private const int _maxLogLines = 200;

    private static TestMenuUI _instanse;

    private GameObject _panel;
    private Text _logText;
    private readonly List<string> _logLines = new List<string>();

    /// <summary>Creates the TEST menu once the campaign selection scene has loaded.</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (_instanse != null)
            return;

        if (SceneManager.GetActiveScene().name != _campaignSelectionSceneName)
            return;

        GameObject menuObject = new GameObject(nameof(TestMenuUI));
        menuObject.AddComponent<TestMenuUI>();
    }

    private void Awake()
    {
        if (_instanse != null)
        {
            Destroy(gameObject);
            return;
        }

        _instanse = this;
        CreateUi();
    }

    private void OnDestroy()
    {
        if (_instanse == this)
            _instanse = null;
    }

    private void CreateUi()
    {
        RuntimeUiFactory.EnsureEventSystem();

        Canvas canvas = RuntimeUiFactory.CreateCanvas("TestMenuCanvas", transform, _sortingOrder);
        CreateToggleButton(canvas.transform);
        CreatePanel(canvas.transform);
        _panel.SetActive(false);
    }

    private void CreateToggleButton(Transform parent)
    {
        GameObject buttonObject = RuntimeUiFactory.CreateUiObject("TestButton", parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 0);
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(1, 0);
        rect.anchoredPosition = new Vector2(-24, 24);
        rect.sizeDelta = new Vector2(120, 48);

        Image background = buttonObject.AddComponent<Image>();
        background.color = RuntimeUiFactory.ToColor(UiStyle.PanelBackground);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(TogglePanel);

        RuntimeUiFactory.CreateText("TestButtonLabel", buttonObject.transform, "TEST",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(110, 44),
            UiStyle.Body, UiStyle.Label);
    }

    private void CreatePanel(Transform parent)
    {
        _panel = RuntimeUiFactory.CreateUiObject("TestPanel", parent);
        RectTransform rect = _panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0, 0);
        rect.sizeDelta = new Vector2(640, 700);

        Image background = _panel.AddComponent<Image>();
        background.color = RuntimeUiFactory.ToColor(UiStyle.PanelBackground);

        RuntimeUiFactory.CreateText("TestPanelTitle", _panel.transform, _panelTitleLabel,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -20), new Vector2(600, 40),
            UiStyle.Title, UiStyle.Label);

        IReadOnlyList<TestActionDefinition> actions = TestActions.Actions;
        for (int i = 0; i < actions.Count; i++)
            CreateActionButton(_panel.transform, actions[i], i);

        CreateClearButton(_panel.transform);
        CreateLogArea(_panel.transform);
    }

    private void CreateActionButton(Transform parent, TestActionDefinition action, int index)
    {
        GameObject buttonObject = RuntimeUiFactory.CreateUiObject("Action_" + index, parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0, -70 - index * 52);
        rect.sizeDelta = new Vector2(600, 44);

        Image background = buttonObject.AddComponent<Image>();
        background.color = RuntimeUiFactory.ToColor(UiStyle.ButtonBackground);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(() => RunAction(action));

        RuntimeUiFactory.CreateText("ActionLabel_" + index, buttonObject.transform, action.Name,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(590, 40),
            UiStyle.Small, UiStyle.Label);
    }

    private void CreateClearButton(Transform parent)
    {
        GameObject buttonObject = RuntimeUiFactory.CreateUiObject("TestClearButton", parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0, 24);
        rect.sizeDelta = new Vector2(100, 36);

        Image background = buttonObject.AddComponent<Image>();
        background.color = RuntimeUiFactory.ToColor(UiStyle.ButtonBackground);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(ClearLog);

        RuntimeUiFactory.CreateText("TestClearLabel", buttonObject.transform, _clearButtonLabel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(90, 32),
            UiStyle.Small, UiStyle.Label);
    }

    private void CreateLogArea(Transform parent)
    {
        _logText = RuntimeUiFactory.CreateText("TestLog", parent, "",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 76), new Vector2(600, 220),
            UiStyle.LogBody, UiStyle.Label, TextAnchor.UpperLeft);
        _logText.horizontalOverflow = HorizontalWrapMode.Wrap;
        _logText.verticalOverflow = VerticalWrapMode.Overflow;
    }

    private void TogglePanel()
    {
        if (_panel == null)
            return;

        _panel.SetActive(!_panel.activeSelf);
    }

    private void RunAction(TestActionDefinition action)
    {
        string result;
        try
        {
            result = action.Run();
        }
        catch (System.Exception ex)
        {
            result = "ERROR: " + ex.Message;
        }

        AppendLog("[" + action.Name + "] " + result);
        Debug.Log("[DD] [TEST] " + action.Name + ": " + result);
    }

    private void AppendLog(string line)
    {
        _logLines.Add(line);
        if (_logLines.Count > _maxLogLines)
            _logLines.RemoveAt(0);
        RefreshLog();
    }

    private void ClearLog()
    {
        _logLines.Clear();
        RefreshLog();
    }

    private void RefreshLog()
    {
        if (_logText != null)
            _logText.text = string.Join("\n", _logLines);
    }
}
