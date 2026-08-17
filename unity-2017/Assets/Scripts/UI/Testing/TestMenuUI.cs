using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Sektor.DarkestDungeon.Core.Ui;

/// <summary>
/// Shell of the runtime-created TEST menu shown on the campaign selection screen. Owns the
/// persistent object, the bottom-right "TEST" button and the panel layout; delegates the log area
/// to <see cref="TestLogView"/> and the content browser to <see cref="TestBrowserView"/>.
/// </summary>
public class TestMenuUI : MonoBehaviour
{
    private const string _campaignSelectionSceneName = "CampaignSelection";
    private const int _sortingOrder = 15000;

    private static TestMenuUI _instanse;

    private Canvas _canvas;
    private GameObject _panel;
    private TestLogView _logView;

    /// <summary>
    /// Creates the persistent TEST menu object on the first scene load. The menu lives across
    /// scenes (DontDestroyOnLoad); its canvas is shown only on the campaign selection screen.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (_instanse != null)
            return;

        GameObject menuObject = new GameObject(nameof(TestMenuUI));
        DontDestroyOnLoad(menuObject);
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

    private void Update()
    {
        bool visible = SceneManager.GetActiveScene().name == _campaignSelectionSceneName;
        if (_canvas != null && _canvas.gameObject.activeSelf != visible)
            _canvas.gameObject.SetActive(visible);
    }

    private void OnDestroy()
    {
        if (_instanse == this)
            _instanse = null;
    }

    private void CreateUi()
    {
        RuntimeUiFactory.EnsureEventSystem();

        _canvas = RuntimeUiFactory.CreateCanvas("TestMenuCanvas", transform, _sortingOrder);
        CreateToggleButton(_canvas.transform);
        CreatePanel(_canvas.transform);
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
        rect.sizeDelta = new Vector2(1520, 820);

        Image background = _panel.AddComponent<Image>();
        background.color = RuntimeUiFactory.ToColor(UiStyle.PanelBackground);

        CreateLeftColumn(_panel.transform);
        new TestBrowserView(_panel.transform, _logView);
    }

    private void CreateLeftColumn(Transform parent)
    {
        RuntimeUiFactory.CreateText("TestActionsTitle", parent, "TEST ACTIONS",
            new Vector2(0, 1f), new Vector2(0, 1f), new Vector2(30, -16), new Vector2(460, 34),
            UiStyle.Title, UiStyle.Label);

        IReadOnlyList<TestActionDefinition> actions = TestActions.Actions;
        for (int i = 0; i < actions.Count; i++)
            CreateActionButton(parent, actions[i], i);

        _logView = new TestLogView(parent, new Vector2(30, -420), new Vector2(460, 360));
        CreateClearButton(parent, new Vector2(370, -792));
    }

    private void CreateActionButton(Transform parent, TestActionDefinition action, int index)
    {
        GameObject buttonObject = RuntimeUiFactory.CreateUiObject("Action_" + index, parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1f);
        rect.anchorMax = new Vector2(0, 1f);
        rect.pivot = new Vector2(0, 1f);
        rect.anchoredPosition = new Vector2(30, -52 - index * 46);
        rect.sizeDelta = new Vector2(460, 40);

        Image background = buttonObject.AddComponent<Image>();
        background.color = RuntimeUiFactory.ToColor(UiStyle.ButtonBackground);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(() => RunAction(action));

        RuntimeUiFactory.CreateText("ActionLabel_" + index, buttonObject.transform, action.Name,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(450, 36),
            UiStyle.Small, UiStyle.Label);
    }

    private void CreateClearButton(Transform parent, Vector2 position)
    {
        GameObject buttonObject = RuntimeUiFactory.CreateUiObject("TestClearButton", parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1f);
        rect.anchorMax = new Vector2(0, 1f);
        rect.pivot = new Vector2(0, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(120, 30);

        Image background = buttonObject.AddComponent<Image>();
        background.color = RuntimeUiFactory.ToColor(UiStyle.ButtonBackground);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(ClearLog);

        RuntimeUiFactory.CreateText("TestClearLabel", buttonObject.transform, "Clear",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(110, 26),
            UiStyle.Small, UiStyle.Label);
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

        _logView.Append("[" + action.Name + "] " + result);
        Debug.Log("[DD] [TEST] " + action.Name + ": " + result);
    }

    private void ClearLog()
    {
        if (_logView != null)
            _logView.Clear();
    }
}
