using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Sektor.DarkestDungeon.Core.Ui;

/// <summary>
/// Runtime-created TEST menu shown on the campaign selection screen. A bottom-right "TEST" button
/// opens a wide panel: left column lists the test actions and a scrollable, clipped log; the right
/// column lets you browse trinkets/curios/diseases/narration sounds and shows a detail (image,
/// name/source and optional sound) for the selected entry.
/// </summary>
public class TestMenuUI : MonoBehaviour
{
    private const string _campaignSelectionSceneName = "CampaignSelection";
    private const int _sortingOrder = 15000;
    private const int _maxLogLines = 200;
    private const int _maxBrowseRows = 60;

    private static TestMenuUI _instanse;

    private Canvas _canvas;
    private GameObject _panel;

    private Text _logText;
    private RectTransform _logContent;

    private RectTransform _browseContent;
    private string _currentCategory;

    private Image _detailImage;
    private Text _detailText;
    private Text _soundText;

    private readonly List<string> _logLines = new List<string>();

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

    #region UI construction

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
        CreateRightColumn(_panel.transform);
    }

    private void CreateLeftColumn(Transform parent)
    {
        RuntimeUiFactory.CreateText("TestActionsTitle", parent, "TEST ACTIONS",
            new Vector2(0, 1f), new Vector2(0, 1f), new Vector2(30, -16), new Vector2(460, 34),
            UiStyle.Title, UiStyle.Label);

        IReadOnlyList<TestActionDefinition> actions = TestActions.Actions;
        for (int i = 0; i < actions.Count; i++)
            CreateActionButton(parent, actions[i], i);

        CreateLogScroll(parent, new Vector2(30, -420), new Vector2(460, 360));
        CreateClearButton(parent, new Vector2(370, -792));
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

    private void CreateLogScroll(Transform parent, Vector2 position, Vector2 size)
    {
        GameObject viewport = RuntimeUiFactory.CreateUiObject("TestLogViewport", parent);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = new Vector2(0, 1f);
        viewportRect.anchorMax = new Vector2(0, 1f);
        viewportRect.pivot = new Vector2(0, 1f);
        viewportRect.anchoredPosition = position;
        viewportRect.sizeDelta = size;

        Image mask = viewport.AddComponent<Image>();
        mask.color = new Color(0, 0, 0, 0.35f);
        mask.raycastTarget = true;
        viewport.AddComponent<RectMask2D>();

        GameObject contentObject = RuntimeUiFactory.CreateUiObject("TestLogContent", viewportRect);
        _logContent = contentObject.GetComponent<RectTransform>();
        _logContent.anchorMin = new Vector2(0, 1);
        _logContent.anchorMax = new Vector2(1, 1);
        _logContent.pivot = new Vector2(0.5f, 1);
        _logContent.anchoredPosition = new Vector2(0, 0);
        _logContent.sizeDelta = new Vector2(0, size.y);

        _logText = RuntimeUiFactory.CreateText("TestLogText", _logContent, "",
            new Vector2(0, 1f), new Vector2(0, 1f), new Vector2(0, 0), new Vector2(size.x, size.y),
            UiStyle.LogBody, UiStyle.Label, TextAnchor.UpperLeft);
        _logText.horizontalOverflow = HorizontalWrapMode.Wrap;
        _logText.verticalOverflow = VerticalWrapMode.Overflow;

        ScrollRect scrollRect = viewport.AddComponent<ScrollRect>();
        scrollRect.content = _logContent;
        scrollRect.viewport = viewportRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
    }

    private void CreateRightColumn(Transform parent)
    {
        RuntimeUiFactory.CreateText("TestDetailTitle", parent, "DETAIL",
            new Vector2(0, 1f), new Vector2(0, 1f), new Vector2(530, -16), new Vector2(960, 34),
            UiStyle.Title, UiStyle.Label);

        CreateCategoryButton(parent, "Trinkets", new Vector2(530, -56));
        CreateCategoryButton(parent, "Curios", new Vector2(710, -56));
        CreateCategoryButton(parent, "Diseases", new Vector2(890, -56));
        CreateCategoryButton(parent, "Sounds", new Vector2(1070, -56));

        CreateBrowseScroll(parent, new Vector2(530, -104), new Vector2(960, 300));

        _detailImage = CreateDetailImage(parent, new Vector2(540, -430));
        _detailText = RuntimeUiFactory.CreateText("TestDetailText", parent, "",
            new Vector2(0, 1f), new Vector2(0, 1f), new Vector2(810, -420), new Vector2(680, 120),
            UiStyle.LogBody, UiStyle.Label, TextAnchor.UpperLeft);
        _detailText.horizontalOverflow = HorizontalWrapMode.Wrap;
        _detailText.verticalOverflow = VerticalWrapMode.Overflow;

        RuntimeUiFactory.CreateText("TestSoundTitle", parent, "SOUND",
            new Vector2(0, 1f), new Vector2(0, 1f), new Vector2(530, -580), new Vector2(300, 26),
            UiStyle.Small, UiStyle.Label);
        _soundText = RuntimeUiFactory.CreateText("TestSoundText", parent, "",
            new Vector2(0, 1f), new Vector2(0, 1f), new Vector2(530, -612), new Vector2(950, 60),
            UiStyle.LogBody, UiStyle.Label, TextAnchor.UpperLeft);
        _soundText.horizontalOverflow = HorizontalWrapMode.Wrap;
        _soundText.verticalOverflow = VerticalWrapMode.Overflow;
    }

    private void CreateCategoryButton(Transform parent, string label, Vector2 position)
    {
        GameObject buttonObject = RuntimeUiFactory.CreateUiObject("Category_" + label, parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1f);
        rect.anchorMax = new Vector2(0, 1f);
        rect.pivot = new Vector2(0, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(160, 36);

        Image background = buttonObject.AddComponent<Image>();
        background.color = RuntimeUiFactory.ToColor(UiStyle.ButtonBackground);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(() => BrowseCategory(label));

        RuntimeUiFactory.CreateText("CategoryLabel_" + label, buttonObject.transform, label,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(150, 32),
            UiStyle.Small, UiStyle.Label);
    }

    private void CreateBrowseScroll(Transform parent, Vector2 position, Vector2 size)
    {
        GameObject viewport = RuntimeUiFactory.CreateUiObject("TestBrowseViewport", parent);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = new Vector2(0, 1f);
        viewportRect.anchorMax = new Vector2(0, 1f);
        viewportRect.pivot = new Vector2(0, 1f);
        viewportRect.anchoredPosition = position;
        viewportRect.sizeDelta = size;

        Image mask = viewport.AddComponent<Image>();
        mask.color = new Color(0, 0, 0, 0.35f);
        mask.raycastTarget = true;
        viewport.AddComponent<RectMask2D>();

        GameObject contentObject = RuntimeUiFactory.CreateUiObject("TestBrowseContent", viewportRect);
        _browseContent = contentObject.GetComponent<RectTransform>();
        _browseContent.anchorMin = new Vector2(0, 1);
        _browseContent.anchorMax = new Vector2(1, 1);
        _browseContent.pivot = new Vector2(0.5f, 1);
        _browseContent.anchoredPosition = new Vector2(0, 0);
        _browseContent.sizeDelta = new Vector2(0, size.y);

        ScrollRect scrollRect = viewport.AddComponent<ScrollRect>();
        scrollRect.content = _browseContent;
        scrollRect.viewport = viewportRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
    }

    private Image CreateDetailImage(Transform parent, Vector2 position)
    {
        GameObject imageObject = RuntimeUiFactory.CreateUiObject("TestDetailImage", parent);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1f);
        rect.anchorMax = new Vector2(0, 1f);
        rect.pivot = new Vector2(0, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(240, 240);

        Image image = imageObject.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0.35f);
        return image;
    }

    #endregion

    #region Interaction

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

    private void BrowseCategory(string category)
    {
        _currentCategory = category;
        ClearBrowseList();

        List<string> entries = ResolveCategoryEntries(category);
        int count = Mathf.Min(entries.Count, _maxBrowseRows);
        for (int i = 0; i < count; i++)
            CreateBrowseRow(entries[i], i);

        _browseContent.sizeDelta = new Vector2(0, Mathf.Max(count * 32, 300));
        AppendLog("[Browse " + category + "] " + entries.Count + " entries");
    }

    private List<string> ResolveCategoryEntries(string category)
    {
        var data = DarkestDungeonManager.Data;
        switch (category)
        {
            case "Trinkets":
                if (data.Items != null && data.Items.ContainsKey("trinket"))
                    return data.Items["trinket"].Keys.OrderBy(id => id).ToList();
                return new List<string>();
            case "Curios":
                if (data.Curios != null)
                    return data.Curios.Keys.OrderBy(id => id).ToList();
                return new List<string>();
            case "Diseases":
                if (data.Quirks != null)
                    return data.Quirks.Values.Where(q => q.IsDisease).Select(q => q.Id).OrderBy(id => id).ToList();
                return new List<string>();
            case "Sounds":
                if (data.Narration != null)
                    return data.Narration.Values.SelectMany(e => e.AudioEvents)
                        .Select(a => a.AudioEvent).Where(p => !string.IsNullOrEmpty(p))
                        .Distinct().OrderBy(p => p).ToList();
                return new List<string>();
            default:
                return new List<string>();
        }
    }

    private void CreateBrowseRow(string entry, int index)
    {
        GameObject rowObject = RuntimeUiFactory.CreateUiObject("Browse_" + index, _browseContent);
        RectTransform rect = rowObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1f);
        rect.anchorMax = new Vector2(1, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0, -index * 32);
        rect.sizeDelta = new Vector2(0, 30);

        Image background = rowObject.AddComponent<Image>();
        background.color = RuntimeUiFactory.ToColor(UiStyle.IdleRow);

        Button button = rowObject.AddComponent<Button>();
        button.targetGraphic = background;
        string captured = entry;
        button.onClick.AddListener(() => ShowDetail(captured));

        RuntimeUiFactory.CreateText("BrowseLabel_" + index, rowObject.transform, entry,
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(8, 0), new Vector2(0, 28),
            UiStyle.Small, UiStyle.Label, TextAnchor.MiddleLeft);
    }

    private void ClearBrowseList()
    {
        for (int i = _browseContent.childCount - 1; i >= 0; i--)
            Destroy(_browseContent.GetChild(i).gameObject);
    }

    private void ShowDetail(string entry)
    {
        string imagePath = "";
        string detail = _currentCategory + ": " + entry;

        switch (_currentCategory)
        {
            case "Trinkets":
                imagePath = "Sprites/Shared/Inventory/Trinket/inv_trinket+" + entry;
                break;
            case "Curios":
                detail += " results=" + DarkestDungeonManager.Data.Curios[entry].Results.Count
                    + " itemInteractions=" + DarkestDungeonManager.Data.Curios[entry].ItemInteractions.Count;
                break;
            case "Sounds":
                PlaySound(entry);
                break;
        }

        SetDetailImage(imagePath);
        _detailText.text = imagePath.Length > 0 ? detail + "\nfile: " + imagePath : detail;
    }

    private void PlaySound(string eventPath)
    {
        try
        {
            DarkestSoundManager.PlayOneShot(eventPath);
            _soundText.text = eventPath;
        }
        catch (System.Exception ex)
        {
            _soundText.text = "Play failed: " + ex.Message;
        }
    }

    private void SetDetailImage(string resourcePath)
    {
        if (string.IsNullOrEmpty(resourcePath))
        {
            _detailImage.sprite = null;
            _detailImage.color = new Color(0, 0, 0, 0.35f);
            return;
        }

        Sprite sprite = Resources.Load<Sprite>(resourcePath);
        if (sprite != null)
        {
            _detailImage.sprite = sprite;
            _detailImage.color = Color.white;
        }
        else
        {
            _detailImage.sprite = null;
            _detailImage.color = new Color(0.2f, 0.2f, 0.2f, 0.6f);
        }
    }

    #endregion

    #region Log

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
        if (_logText == null)
            return;

        _logText.text = string.Join("\n", _logLines);
        _logContent.sizeDelta = new Vector2(0, Mathf.Max(_logText.preferredHeight, 360));
    }

    #endregion
}
