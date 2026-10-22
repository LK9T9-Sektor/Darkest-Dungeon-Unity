using System.Collections.Generic;
using System.Windows.Media;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Inert tile used to reserve a skill slot when the content has fewer than seven entries.</summary>
    public sealed class EmptySkillTileViewModel : ISkillTileViewModel
    {
        /// <summary>Gets the shared empty tile.</summary>
        public static EmptySkillTileViewModel Instance { get; } = new EmptySkillTileViewModel();

        private EmptySkillTileViewModel()
        {
        }

        /// <inheritdoc/>
        public ImageSource? Icon { get { return null; } }

        /// <inheritdoc/>
        public string DisplayNameUpper { get { return string.Empty; } }

        /// <inheritdoc/>
        public bool IsUsable { get { return false; } }

        /// <inheritdoc/>
        public bool IsSelected { get { return false; } }

        /// <inheritdoc/>
        public bool IsLoadout { get { return true; } }

        /// <inheritdoc/>
        public bool IsMasked { get { return true; } }

        /// <summary>Gets the hidden level badge value.</summary>
        public int Level { get { return 0; } }

        /// <summary>Gets the empty tooltip name.</summary>
        public string DisplayName { get { return string.Empty; } }

        /// <summary>Gets the empty tooltip base text.</summary>
        public string BaseInfo { get { return string.Empty; } }

        /// <summary>Gets the empty tooltip effect rows.</summary>
        public List<SkillEffectRowViewModel> EffectRows { get; } = new List<SkillEffectRowViewModel>();
    }
}
