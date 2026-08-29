namespace Sektor.DarkestDungeon.Core.Campaign.Database
{
    /// <summary>A prerequisite pointing to another upgrade tree level.</summary>
    public class JsonPrerequisiteRequirement
    {
        /// <summary>Gets or sets the prerequisite tree id.</summary>
        public string tree_id { get; set; }

        /// <summary>Gets or sets the prerequisite requirement code.</summary>
        public string requirement_code { get; set; }
    }
}