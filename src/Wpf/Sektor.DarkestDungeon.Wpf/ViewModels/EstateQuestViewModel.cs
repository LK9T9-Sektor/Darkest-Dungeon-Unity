using Sektor.DarkestDungeon.Core.GameEvents;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>One quest card in the estate replay.</summary>
    public sealed class EstateQuestViewModel
    {
        /// <summary>Initializes a new instance of the <see cref="EstateQuestViewModel"/> class.</summary>
        /// <param name="quest">The quest state.</param>
        public EstateQuestViewModel(EstateQuestState quest)
        {
            Id = quest.Id;
            Title = $"{quest.Dungeon} · {quest.Type}";
            Details = $"Сложность {quest.Difficulty} · длина {quest.Length}";
            Goal = quest.GoalId;
            IsPlot = quest.IsPlot ? "СЮЖЕТ" : string.Empty;
        }

        /// <summary>Gets the quest id.</summary>
        public string Id { get; }

        /// <summary>Gets the quest title.</summary>
        public string Title { get; }

        /// <summary>Gets the difficulty/length line.</summary>
        public string Details { get; }

        /// <summary>Gets the goal id.</summary>
        public string Goal { get; }

        /// <summary>Gets the plot marker.</summary>
        public string IsPlot { get; }
    }
}
