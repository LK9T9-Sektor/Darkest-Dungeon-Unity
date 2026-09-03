using UnityEngine;
using UnityEngine.UI;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;

/// <summary>
/// A single battle unit visual: the Spine unit prefab (with the legacy <see cref="FormationUnit"/>
/// facade removed), scaled at runtime to a fixed world height so every unit renders at the same size,
/// plus world-space health/stress bars and a target selection frame. Driven by the core battle state;
/// no battle logic lives here.
/// </summary>
public class BattleUnitView : MonoBehaviour
{
    private const float TargetHeight = 8f;
    private const int MaxBuildFrames = 10;

    private RectTransform _rect;
    private Image _healthFill;
    private Image _stressFill;
    private Image _selection;
    private int _combatId;
    private bool _isHero;
    private float _unitScale = 1f;
    private bool _built;
    private int _buildFrame;
    private bool _dead;

    /// <summary>Gets the unit combat id this view renders.</summary>
    public int CombatId { get { return _combatId; } }

    /// <summary>Gets the world position of the unit body.</summary>
    public Vector3 WorldPosition { get { return _rect != null ? _rect.position : Vector3.zero; } }

    /// <summary>Creates a unit view under the given parent.</summary>
    /// <param name="classId">The hero or monster class id.</param>
    /// <param name="isHero">Whether the unit is a hero (affects prefab folder and bar colour).</param>
    /// <param name="parent">The world-space canvas rect to parent under.</param>
    /// <param name="combatId">The core combat id.</param>
    /// <param name="flipFacing">Whether the unit's visual must face the opposite direction (hero on the right side).</param>
    public static BattleUnitView Create(string classId, bool isHero, Transform parent, int combatId, bool flipFacing)
    {
        GameObject prefab = Resources.Load<GameObject>(isHero ? "Prefabs/Heroes/" + classId : "Prefabs/Monsters/" + classId);
        GameObject body;

        if (prefab != null)
        {
            body = Instantiate(prefab);
            FormationUnit legacy = body.GetComponent<FormationUnit>();
            if (legacy != null)
            {
                legacy.enabled = false;
                Destroy(legacy);
            }
        }
        else
        {
            body = new GameObject(classId);
            body.AddComponent<RectTransform>();
            Image placeholder = body.AddComponent<Image>();
            placeholder.color = isHero ? new Color(0.3f, 0.5f, 0.9f, 0.9f) : new Color(0.9f, 0.35f, 0.3f, 0.9f);
        }

        body.transform.SetParent(parent, false);

        if (flipFacing)
            FlipFacing(body);

        BattleUnitView view = body.GetComponent<BattleUnitView>();
        if (view == null)
            view = body.AddComponent<BattleUnitView>();
        view._combatId = combatId;
        view._isHero = isHero;
        return view;
    }

    private void Update()
    {
        if (_built)
            return;

        _buildFrame++;
        if (TryNormalizeHeight())
        {
            BuildWidgets();
            _built = true;
            return;
        }

        if (_buildFrame >= MaxBuildFrames)
        {
            _unitScale = 1f;
            BuildWidgets();
            _built = true;
        }
    }

    private static void FlipFacing(GameObject body)
    {
        UnitAnimator animator = body.GetComponentInChildren<UnitAnimator>();
        if (animator == null)
            return;

        Vector3 scale = animator.transform.localScale;
        scale.x = -Mathf.Abs(scale.x);
        animator.transform.localScale = scale;
    }

    private bool TryNormalizeHeight()
    {
        float height = GetVisualHeight();
        if (height <= 0.001f)
            return false;

        float factor = TargetHeight / height;
        transform.localScale = new Vector3(
            transform.localScale.x * factor,
            transform.localScale.y * factor,
            transform.localScale.z * factor);
        _unitScale = transform.localScale.x;
        return true;
    }

    private float GetVisualHeight()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return 0f;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        return bounds.size.y;
    }

    private void BuildWidgets()
    {
        _rect = GetComponent<RectTransform>();
        float s = _unitScale;

        _rect.sizeDelta = new Vector2(6f / s, TargetHeight / s);

        Color healthColor = _isHero ? new Color(0.4f, 0.95f, 0.35f) : new Color(0.95f, 0.35f, 0.3f);
        _selection = CreateBar("Selection", new Vector2(6.5f / s, 9f / s), Vector2.zero,
            new Color(1f, 0.85f, 0.2f, 0.35f));
        _healthFill = CreateBar("HealthFill", new Vector2(5f / s, 0.4f / s), new Vector2(0f, -3.6f / s), healthColor);
        _stressFill = CreateBar("StressFill", new Vector2(5f / s, 0.25f / s), new Vector2(0f, -3.1f / s),
            new Color(1f, 1f, 1f, 0.9f));
    }

    private Image CreateBar(string name, Vector2 size, Vector2 offset, Color color)
    {
        GameObject barObject = new GameObject(name);
        barObject.transform.SetParent(transform, false);
        RectTransform barRect = barObject.AddComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0.5f, 0.5f);
        barRect.anchorMax = new Vector2(0.5f, 0.5f);
        barRect.pivot = new Vector2(0.5f, 0.5f);
        barRect.anchoredPosition = offset;
        barRect.sizeDelta = size;

        Image background = barObject.AddComponent<Image>();
        background.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        GameObject fillObject = new GameObject("Fill");
        fillObject.transform.SetParent(barObject.transform, false);
        RectTransform fillRect = fillObject.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Image fill = fillObject.AddComponent<Image>();
        fill.color = color;
        return fill;
    }

    /// <summary>Refreshes the view from the current core unit state.</summary>
    /// <param name="unit">The core unit.</param>
    public void UpdateFrom(ICombatUnit unit)
    {
        if (unit == null || unit.Character == null)
            return;

        if (unit.CombatInfo.IsDead)
        {
            ShowDeath();
            return;
        }

        if (_healthFill != null)
            _healthFill.rectTransform.localScale = new Vector3(Mathf.Clamp01(unit.Character.HealthRatio), 1f, 1f);

        if (_stressFill != null)
        {
            float stress = unit.Character.Stress != null ? unit.Character.Stress.CurrentValue : 0f;
            _stressFill.rectTransform.localScale = new Vector3(Mathf.Clamp01(stress / 100f), 1f, 1f);
        }
    }

    /// <summary>Positions the unit at the given world position.</summary>
    /// <param name="worldPosition">The target world position.</param>
    public void SetPosition(Vector3 worldPosition)
    {
        if (_rect != null)
            _rect.position = worldPosition;
    }

    /// <summary>Toggles the target selection frame.</summary>
    /// <param name="highlight">Whether to highlight the unit as a valid target.</param>
    public void SetHighlight(bool highlight)
    {
        if (_selection != null)
            _selection.gameObject.SetActive(highlight);
    }

    /// <summary>Hides the unit body and bars (death).</summary>
    public void ShowDeath()
    {
        if (_dead)
            return;
        _dead = true;
        gameObject.SetActive(false);
    }
}