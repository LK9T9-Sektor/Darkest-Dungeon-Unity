using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Sektor.DarkestDungeon.Core.Ui;

/// <summary>
/// The scrollable log area of the TEST menu. Clips its content (RectMask2D) so long lines cannot
/// overflow the panel and pins the scroll to the newest entry.
/// </summary>
public class TestLogView
{
    private const int MaxLines = 200;

    private readonly List<string> _lines = new List<string>();
    private readonly Text _logText;
    private readonly RectTransform _content;
    private readonly ScrollRect _scrollRect;

    /// <summary>Initializes a new instance of the <see cref="TestLogView"/> class and builds its UI.</summary>
    /// <param name="parent">The parent transform (panel).</param>
    /// <param name="position">The anchored position of the viewport (top-left anchored).</param>
    /// <param name="size">The viewport size.</param>
    public TestLogView(Transform parent, Vector2 position, Vector2 size)
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
        _content = contentObject.GetComponent<RectTransform>();
        _content.anchorMin = new Vector2(0, 1);
        _content.anchorMax = new Vector2(1, 1);
        _content.pivot = new Vector2(0.5f, 1);
        _content.anchoredPosition = new Vector2(0, 0);
        _content.sizeDelta = new Vector2(0, size.y);

        _logText = RuntimeUiFactory.CreateText("TestLogText", _content, "",
            new Vector2(0, 1f), new Vector2(0, 1f), new Vector2(0, 0), new Vector2(size.x, size.y),
            UiStyle.LogBody, UiStyle.Label, TextAnchor.UpperLeft);
        _logText.horizontalOverflow = HorizontalWrapMode.Wrap;
        _logText.verticalOverflow = VerticalWrapMode.Overflow;

        _scrollRect = viewport.AddComponent<ScrollRect>();
        _scrollRect.content = _content;
        _scrollRect.viewport = viewportRect;
        _scrollRect.horizontal = false;
        _scrollRect.vertical = true;
        _scrollRect.movementType = ScrollRect.MovementType.Clamped;
    }

    /// <summary>Appends a line and keeps the newest entry visible.</summary>
    /// <param name="line">The log line.</param>
    public void Append(string line)
    {
        _lines.Add(line);
        if (_lines.Count > MaxLines)
            _lines.RemoveAt(0);
        Refresh();
    }

    /// <summary>Clears all log lines.</summary>
    public void Clear()
    {
        _lines.Clear();
        Refresh();
    }

    private void Refresh()
    {
        _logText.text = string.Join("\n", _lines);
        _content.sizeDelta = new Vector2(0, Mathf.Max(_logText.preferredHeight, 360));
        _scrollRect.verticalNormalizedPosition = 0;
    }
}
