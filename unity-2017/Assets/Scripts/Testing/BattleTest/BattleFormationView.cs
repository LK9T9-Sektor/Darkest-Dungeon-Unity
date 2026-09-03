using System.Collections.Generic;
using UnityEngine;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Party;

/// <summary>
/// Renders one formation (hero or monster side) as a row of <see cref="BattleUnitView"/> instances laid
/// out with <see cref="FormationDisplayOrder"/> and refreshed from the core party state on every change.
/// </summary>
public class BattleFormationView : MonoBehaviour
{
    private readonly Dictionary<int, BattleUnitView> _views = new Dictionary<int, BattleUnitView>();

    private IFormationParty _party;
    private FormationDisplayOrder _order;
    private Vector3 _origin;
    private float _spacing;
    private bool _initialized;

    /// <summary>Initializes the formation for a core party.</summary>
    /// <param name="party">The core formation party.</param>
    /// <param name="order">The display order rule for this side.</param>
    /// <param name="origin">The world position of the leftmost slot.</param>
    /// <param name="spacing">The horizontal distance between slots.</param>
    public void Initialize(IFormationParty party, FormationDisplayOrder order, Vector3 origin, float spacing)
    {
        _party = party;
        _order = order;
        _origin = origin;
        _spacing = spacing;
        _initialized = true;

        foreach (Transform child in transform)
            Destroy(child.gameObject);
        _views.Clear();
    }

    /// <summary>Reconciles and lays out all unit views from the current core party state.</summary>
    public void UpdateUnits()
    {
        if (!_initialized || _party == null)
            return;

        var aliveInParty = new HashSet<int>();
        var ordered = _order.OrderLeftToRight(_party);

        for (int i = 0; i < ordered.Count; i++)
        {
            ICombatUnit unit = ordered[i];
            int combatId = unit.CombatInfo.CombatId;
            aliveInParty.Add(combatId);

            BattleUnitView view = EnsureView(unit);
            view.SetPosition(_origin + new Vector3(i * _spacing, 0f, 0f));
            view.UpdateFrom(unit);
        }

        var dead = new List<int>();
        foreach (int combatId in _views.Keys)
        {
            if (!aliveInParty.Contains(combatId))
                dead.Add(combatId);
        }
        foreach (int combatId in dead)
        {
            BattleUnitView view;
            if (_views.TryGetValue(combatId, out view) && view != null)
                view.ShowDeath();
            _views.Remove(combatId);
        }
    }

    /// <summary>Gets the view for a combat id, or null when unknown.</summary>
    /// <param name="combatId">The combat id.</param>
    public BattleUnitView GetView(int combatId)
    {
        BattleUnitView view;
        return _views.TryGetValue(combatId, out view) ? view : null;
    }

    /// <summary>Toggles the target highlight on a unit.</summary>
    /// <param name="combatId">The combat id.</param>
    /// <param name="highlight">Whether to highlight.</param>
    public void SetHighlight(int combatId, bool highlight)
    {
        BattleUnitView view = GetView(combatId);
        if (view != null)
            view.SetHighlight(highlight);
    }

    /// <summary>Clears all target highlights on this side.</summary>
    public void ClearHighlights()
    {
        foreach (BattleUnitView view in _views.Values)
            view.SetHighlight(false);
    }

    private BattleUnitView EnsureView(ICombatUnit unit)
    {
        int combatId = unit.CombatInfo.CombatId;
        BattleUnitView view;
        if (_views.TryGetValue(combatId, out view) && view != null)
            return view;

        bool isHero = !unit.Character.IsMonster;
        view = BattleUnitView.Create(unit.Character.Class, isHero, transform, combatId);
        _views[combatId] = view;
        return view;
    }
}