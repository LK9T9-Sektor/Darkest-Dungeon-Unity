using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Data.Dto
{
    /// <summary>A single monster brain entry of the JsonAI.json content file.</summary>
    public class JsonMonsterBrains
    {
        /// <summary>Gets or sets the brain identifier.</summary>
        public string id { get; set; }

        /// <summary>Gets or sets the skill cooldown entries.</summary>
        public List<JsonSkillCooldown> skill_cooldowns { get; set; }

        /// <summary>Gets or sets the skill selection desires.</summary>
        public List<JsonSelectionDesire> skill_selection_desires { get; set; }

        /// <summary>Gets or sets the target selection desires.</summary>
        public List<JsonSelectionDesire> target_selection_desires { get; set; }

        /// <summary>Gets or sets the bonus initiative desires.</summary>
        public List<JsonSelectionDesire> bonus_initiative_desires { get; set; }
    }
}