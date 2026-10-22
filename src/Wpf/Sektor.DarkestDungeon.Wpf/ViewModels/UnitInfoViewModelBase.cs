using CommunityToolkit.Mvvm.ComponentModel;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Shared presentation contract for the unit-info overlay. Capabilities describe what
    /// the current sheet allows; the overlay binds to them instead of branching on a mode flag.
    /// The base defaults to a fully read-only sheet, so new unit kinds are safe by default.</summary>
    public abstract class UnitInfoViewModelBase : ObservableObject
    {
        /// <summary>Gets a value indicating whether the unit name can be edited.</summary>
        public virtual bool CanEditName { get { return false; } }

        /// <summary>Gets a value indicating whether the unit name can be rerolled.</summary>
        public virtual bool CanRerollName { get { return false; } }

        /// <summary>Gets a value indicating whether combat skills can be selected.</summary>
        public virtual bool CanEditSkills { get { return false; } }

        /// <summary>Gets a value indicating whether camping skills can be selected.</summary>
        public virtual bool CanEditCampingSkills { get { return false; } }

        /// <summary>Gets a value indicating whether the skin selector is available.</summary>
        public virtual bool CanChangeSkin { get { return false; } }

        /// <summary>Gets a value indicating whether trinket reroll buttons are shown.</summary>
        public virtual bool CanRerollTrinkets { get { return false; } }

        /// <summary>Gets a value indicating whether the bottom portrait is shown.</summary>
        public virtual bool ShowPortrait { get { return false; } }

        /// <summary>Gets a value indicating whether the class-switch arrows are shown.</summary>
        public virtual bool ShowClassSwitcher { get { return false; } }

        /// <summary>Gets a value indicating whether the formation position is shown.</summary>
        public virtual bool ShowPosition { get { return false; } }

        /// <summary>Gets a value indicating whether the bottom character controls have any visible part.</summary>
        public bool ShowCharacterControls
        {
            get { return ShowPortrait || CanChangeSkin || ShowClassSwitcher || ShowPosition; }
        }

        /// <summary>Gets a value indicating whether skill tiles are read-only.</summary>
        public bool IsSkillSelectionReadOnly { get { return !CanEditSkills; } }

        /// <summary>Gets a value indicating whether camping-skill tiles are read-only.</summary>
        public bool IsCampingSelectionReadOnly { get { return !CanEditCampingSkills; } }
    }
}
