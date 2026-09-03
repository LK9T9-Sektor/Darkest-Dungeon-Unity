using UnityEngine;
using UnityEngine.UI;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;

/// <summary>
/// A single battle unit visual: the Spine unit prefab (with the legacy <see cref="FormationUnit"/>
/// facade removed) plus world-space health/stress bars and a target selection frame. Driven by the
/// core battle state; no battle logic lives here.
/// </summary>
public class BattleUnitView : MonoBehaviour
{
    private static readonly Vector2 SlotSize = new Vector2(140f, 180f);
    private static readonly Vector2 BarSize = new Vector2(110f, 8f);
    private static readonly Vector2 StressBarSize = new Vector2(110f, 5f);

    private RectTransform _rect;
    private Image _healthFill;
    private Image _stressFill;
    private Image _selection;
    private int _combatId;
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
        view.BuildWidgets(isHero);
        return view;
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

    private void BuildWidgets(bool isHero)
    {
        _rect = GetComponent<RectTransform>();
        _rect.sizeDelta = SlotSize;

        Color healthColor = isHero ? new Color(0.4f, 0.95f, 0.35f) : new Color(0.95f, 0.35f, 0.3f);
        _selection = CreateBar("Selection", new Vector2(150f, 200f), Vector2.zero,
            new Color(1f, 0.85f, 0.2f, 0.35f));
        _healthFill = CreateBar("HealthFill", BarSize, new Vector2(0f, -74f), healthColor);
        _stressFill = CreateBar("StressFill", StressBarSize, new Vector2(0f, -64f),
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