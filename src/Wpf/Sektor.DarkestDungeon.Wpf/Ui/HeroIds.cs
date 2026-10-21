using System.Text;

namespace Sektor.DarkestDungeon.Wpf.Ui
{
    /// <summary>Maps hero content folder names to the canonical snake_case class ids.</summary>
    public static class HeroIds
    {
        /// <summary>Converts a hero folder name to its snake_case class id ("ManAtArms" to "man_at_arms").</summary>
        /// <param name="heroName">The hero folder name or an already snake_case class id.</param>
        /// <returns>The snake_case class id; empty for empty input.</returns>
        public static string ToClassId(string heroName)
        {
            if (string.IsNullOrEmpty(heroName))
                return string.Empty;

            var builder = new StringBuilder(heroName.Length + 4);
            foreach (char current in heroName)
            {
                if (!char.IsUpper(current))
                {
                    builder.Append(current);
                    continue;
                }

                if (builder.Length > 0 && builder[builder.Length - 1] != '_')
                    builder.Append('_');
                builder.Append(char.ToLowerInvariant(current));
            }

            return builder.ToString();
        }
    }
}
