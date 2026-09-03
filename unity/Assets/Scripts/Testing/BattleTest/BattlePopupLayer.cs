using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sektor.DarkestDungeon.Core.Ui;

/// <summary>
/// World-space floating popups (damage numbers, status labels) above the battle units. Each popup is a
/// text raised and faded over ~1.2 seconds and then destroyed.
/// </summary>
public class BattlePopupLayer : MonoBehaviour
{
    private const float CanvasScale = 0.01f;
    private const int FontSize = 120;
    private const float FloatSpeed = 0.6f;
    private const float Lifetime = 1.2f;

    private readonly List<PopupInstance> _popups = new List<PopupInstance>();
    private RectTransform _canvas;
    private bool _initialized;

    /// <summary>Resolves the world position of a unit by combat id.</summary>
    public Func<int, Vector3> WorldPosResolver { get; set; }

    /// <summary>Creates and initializes the popup layer under the given parent.</summary>
    /// <param name="parent">The parent transform (the battlefield root).</param>
    public static BattlePopupLayer Create(Transform parent)
    {
        GameObject layerObject = new GameObject("BattlePopups");
        layerObject.transform.SetParent(parent, false);

        Canvas canvas = layerObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.transform.localScale = new Vector3(CanvasScale, CanvasScale, CanvasScale);

        BattlePopupLayer layer = layerObject.AddComponent<BattlePopupLayer>();
        layer._canvas = canvas.transform as RectTransform;
        layer._initialized = true;
        return layer;
    }

    /// <summary>Shows a popup over the unit with the given combat id.</summary>
    /// <param name="combatId">The unit combat id.</param>
    /// <param name="text">The popup text.</param>
    /// <param name="color">The popup colour.</param>
    public void ShowAt(int combatId, string text, Color color)
    {
        Vector3 position;
        if (WorldPosResolver == null || !TryResolve(combatId, out position))
            return;
        ShowAt(position, text, color);
    }

    /// <summary>Shows a popup at an explicit world position.</summary>
    /// <param name="worldPosition">The world position.</param>
    /// <param name="text">The popup text.</param>
    /// <param name="color">The popup colour.</param>
    public void ShowAt(Vector3 worldPosition, string text, Color color)
    {
        if (!_initialized || string.IsNullOrEmpty(text))
            return;

        Text popup = RuntimeUiFactory.CreateText(
            "Popup", _canvas, text,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400, 200),
            FontSize, UiStyleColor(color), TextAnchor.MiddleCenter);

        popup.rectTransform.position = worldPosition + new Vector3(0f, 0.8f, 0f);
        _popups.Add(new PopupInstance(popup, Time.time));
    }

    private void Update()
    {
        for (int i = _popups.Count - 1; i >= 0; i--)
        {
            PopupInstance popup = _popups[i];
            float age = Time.time - popup.StartTime;
            if (age > Lifetime)
            {
                Destroy(popup.Text.gameObject);
                _popups.RemoveAt(i);
                continue;
            }

            Vector3 position = popup.Text.rectTransform.position;
            position.y += FloatSpeed * Time.deltaTime;
            popup.Text.rectTransform.position = position;

            Color color = popup.Text.color;
            color.a = Mathf.Clamp01(1f - age / Lifetime);
            popup.Text.color = color;
        }
    }

    private bool TryResolve(int combatId, out Vector3 position)
    {
        position = Vector3.zero;
        if (WorldPosResolver == null)
            return false;
        Vector3 resolved = WorldPosResolver(combatId);
        if (resolved == Vector3.zero)
            return false;
        position = resolved;
        return true;
    }

    private static ArgbColor UiStyleColor(Color color)
    {
        return new ArgbColor(
            (byte)(color.a * 255f),
            (byte)(color.r * 255f),
            (byte)(color.g * 255f),
            (byte)(color.b * 255f));
    }

    private sealed class PopupInstance
    {
        public PopupInstance(Text text, float startTime)
        {
            Text = text;
            StartTime = startTime;
        }

        public Text Text { get; }

        public float StartTime { get; }
    }
}