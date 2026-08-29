namespace Sektor.DarkestDungeon.Core.Data.Dto
{
    /// <summary>Skill cooldown entry of a JsonAI.json monster brain.</summary>
    public class JsonSkillCooldown
    {
        /// <summary>Gets or sets the combat skill identifier.</summary>
        public string combat_skill_id { get; set; }

        /// <summary>Gets or sets the cooldown amount.</summary>
        public int amount { get; set; }
    }
}