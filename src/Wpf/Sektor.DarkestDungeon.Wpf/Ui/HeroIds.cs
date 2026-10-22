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

        /// <summary>Converts a snake_case class id to its Spine/icon folder name ("man_at_arms" to
        /// "ManAtArms"). Already PascalCase ids are returned unchanged.</summary>
        /// <param name="classId">The snake_case class id (or an already PascalCase folder name).</param>
        /// <returns>The PascalCase folder name; empty for empty input.</returns>
        public static string ToFolderName(string classId)
        {
            if (string.IsNullOrEmpty(classId))
                return string.Empty;

            var builder = new StringBuilder(classId.Length);
            bool upper = true;
            foreach (char current in classId)
            {
                if (current == '_')
                {
                    upper = true;
                    continue;
                }

                builder.Append(upper ? char.ToUpperInvariant(current) : current);
                upper = false;
            }

            return builder.ToString();
        }
    }
}
