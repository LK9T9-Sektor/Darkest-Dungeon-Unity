using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Sektor.DarkestDungeon.Wpf.Data
{
    /// <summary>Loads the English hero name pool from the bundled localization <c>Names.xml</c>.</summary>
    public static class HeroNameCatalog
    {
        private const string NamesRelativePath = "Content/Names/Names.xml";
        private const string NameIdPrefix = "hero_name_";

        private static readonly IReadOnlyList<string> Names = Load();

        /// <summary>Gets all bundled hero names.</summary>
        public static IReadOnlyList<string> All { get { return Names; } }

        /// <summary>Gets a random hero name, or null when the catalog is empty.</summary>
        /// <param name="random">The random source.</param>
        /// <returns>A name from the bundled pool, or null.</returns>
        public static string? RandomName(Random random)
        {
            if (Names.Count == 0)
                return null;

            return Names[random.Next(Names.Count)];
        }

        private static IReadOnlyList<string> Load()
        {
            string path = Path.Combine(AppContext.BaseDirectory, NamesRelativePath);
            if (!File.Exists(path))
                return Array.Empty<string>();

            try
            {
                var document = XDocument.Load(path);
                var english = document.Root?.Elements("language")
                    .FirstOrDefault(language => string.Equals(
                        (string?)language.Attribute("id"), "english", StringComparison.OrdinalIgnoreCase));
                if (english == null)
                    return Array.Empty<string>();

                return english.Elements("entry")
                    .Where(entry => ((string?)entry.Attribute("id"))?.StartsWith(NameIdPrefix, StringComparison.Ordinal) == true)
                    .Select(entry => entry.Value.Trim())
                    .Where(name => name.Length > 0)
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
            }
            catch (Exception exception) when (exception is IOException
                or UnauthorizedAccessException
                or XmlException
                or ArgumentException)
            {
                return Array.Empty<string>();
            }
        }
    }
}
