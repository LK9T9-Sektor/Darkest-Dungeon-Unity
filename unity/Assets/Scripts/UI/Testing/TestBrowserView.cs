using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Sektor.DarkestDungeon.Core.Ui;

/// <summary>
/// The right column of the TEST menu: category buttons, a scrollable browse list and a detail
/// view (image/text/sound). Category sources are registered per entity
/// (<see cref="TestEntitySource"/>).
/// </summary>
public class TestBrowserView
{
    private const int MaxRows = 60;

    private readonly List<TestEntitySource> _sources = new List<TestEntitySource>();
    private readonly TestLogView _log;
    private readonly RectTransform _browseContent;
    private readonly TestDetailView _detail;

    private string _currentCategory;

    /// <summary>Initializes a new instance of the <see cref="TestBrowserView"/> class and builds its UI.</summary>
    /// <param name="parent">The parent transform (panel).</param>
    /// <param name="log">The shared TEST log for browsing feedback.</param>
    public TestBrowserView(Transform parent, TestLogView log)
    {
        _log = log;

        Register(TestTrinketSource.Create());
        Register(TestCurioSource.Create());
        Register(TestDiseaseSource.Create());
        Register(TestSoundSource.Create());

        for (int i = 0; i < _sources.Count; i++)
            CreateCategoryButton(parent, _sources[i], new Vector2(530 + i * 180, -56));

        _browseContent = CreateBrowseScroll(parent, new Vector2(530, -104), new Vector2(960, 300));

        _detail = new TestDetailView(
            CreateDetailImage(parent, new Vector2(540, -430)),
            RuntimeUiFactory.CreateText("TestDetailText", parent, "",
                new Vector2(0, 1f), new Vector2(0, 1f), new Vector2(810, -420), new Vector2(680, 120),
                UiStyle.LogBody, UiStyle.Label, TextAnchor.UpperLeft),
            CreateSoundText(parent));
    }

    /// <summary>Browses the given category: fills the list with its entries.</summary>
    /// <param name="category">The category display name.</param>
    public void Browse(string category)
    {
        TestEntitySource source = _sources.Find(s => s.Category == category);
        if (source == null)
            return;

        _currentCategory = category;
        ClearList();

        List<string> entries = source.ListEntries();
        int count = Mathf.Min(entries.Count, MaxRows);
        for (int i = 0; i < count; i++)
            CreateBrowseRow(entries[i], i);

        _browseContent.sizeDelta = new Vector2(0, Mathf.Max(count * 32, 300));
        _log.Append("[Browse " + category + "] " + entries.Count + " entries");
    }

    private void Register(TestEntitySource source)
    {
        _sources.Add(source);
    }

    private void CreateCategoryButton(Transform parent, TestEntitySource source, Vector2 position)
    {
        GameObject buttonObject = RuntimeUiFactory.CreateUiObject("Category_" + source.Category, parent);
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
        button.onClick.AddListener(() => Browse(source.Category));

        RuntimeUiFactory.CreateText("CategoryLabel_" + source.Category, buttonObject.transform, source.Category,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(150, 32),
            UiStyle.Small, UiStyle.Label);
    }

    private RectTransform CreateBrowseScroll(Transform parent, Vector2 position, Vector2 size)
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
        RectTransform content = contentObject.GetComponent<RectTransform>();
        content.anchorMin = new Vector2(0, 1);
        content.anchorMax = new Vector2(1, 1);
        content.pivot = new Vector2(0.5f, 1);
        content.anchoredPosition = new Vector2(0, 0);
        content.sizeDelta = new Vector2(0, size.y);

        ScrollRect scrollRect = viewport.AddComponent<ScrollRect>();
        scrollRect.content = content;
        scrollRect.viewport = viewportRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        return content;
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

    private Text CreateSoundText(Transform parent)
    {
        RuntimeUiFactory.CreateText("TestSoundTitle", parent, "SOUND",
            new Vector2(0, 1f), new Vector2(0, 1f), new Vector2(530, -580), new Vector2(300, 26),
            UiStyle.Small, UiStyle.Label);

        Text soundText = RuntimeUiFactory.CreateText("TestSoundText", parent, "",
            new Vector2(0, 1f), new Vector2(0, 1f), new Vector2(530, -612), new Vector2(950, 60),
            UiStyle.LogBody, UiStyle.Label, TextAnchor.UpperLeft);
        soundText.horizontalOverflow = HorizontalWrapMode.Wrap;
        soundText.verticalOverflow = VerticalWrapMode.Overflow;
        return soundText;
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
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(8, 0), new Vector2(944, 28),
            UiStyle.Small, UiStyle.Label, TextAnchor.MiddleLeft);
    }

    private void ShowDetail(string entry)
    {
        TestEntitySource source = _sources.Find(s => s.Category == _currentCategory);
        if (source != null)
            source.ShowDetail(entry, _detail);
    }

    private void ClearList()
    {
        for (int i = _browseContent.childCount - 1; i >= 0; i--)
            Object.Destroy(_browseContent.GetChild(i).gameObject);
    }
}
