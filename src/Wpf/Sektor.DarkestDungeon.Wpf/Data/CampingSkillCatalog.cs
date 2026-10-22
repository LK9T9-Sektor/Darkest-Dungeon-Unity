using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Sektor.DarkestDungeon.Clients.Content;
using Sektor.DarkestDungeon.Core.Content.Camping;

namespace Sektor.DarkestDungeon.Wpf.Data
{
    /// <summary>Loads the camping skill definitions and their icons from the bundled content files.
    /// A missing file or an unknown class yields an empty list, never an exception.</summary>
    public static class CampingSkillCatalog
    {
        private static readonly Core.Content.Camping.CampingSkillCatalog Inner = LoadInner();
        private static readonly Dictionary<string, ImageSource?> IconCache = new Dictionary<string, ImageSource?>();
        private static readonly string IconDirectory = Path.Combine(AppContext.BaseDirectory, "Content", "Camping");

        private static Core.Content.Camping.CampingSkillCatalog LoadInner()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Content", "Camping", "JsonCamping.json");
            return GameDataReader.ReadCampingSkills(File.Exists(path) ? File.ReadAllText(path) : string.Empty);
        }

        /// <summary>Gets the camping skills available to a hero class, ordered by id.</summary>
        /// <param name="classId">The snake_case hero class id.</param>
        /// <returns>The class camping skills; empty when the class is unknown or content is missing.</returns>
        public static IReadOnlyList<CampingSkill> GetForClass(string classId)
        {
            var result = new List<CampingSkill>();
            if (string.IsNullOrEmpty(classId))
                return result;

            foreach (CampingSkill skill in Inner.All)
            {
                if (skill.HeroClasses.Contains(classId, StringComparer.OrdinalIgnoreCase))
                    result.Add(skill);
            }

            result.Sort((left, right) => string.Compare(left.Id, right.Id, StringComparison.OrdinalIgnoreCase));
            return result;
        }

        /// <summary>Gets the bundled camping skill icon by skill id, or null when it is not delivered.</summary>
        /// <param name="skillId">The camping skill id.</param>
        /// <returns>The frozen icon image, or null.</returns>
        public static ImageSource? GetIcon(string skillId)
        {
            if (string.IsNullOrEmpty(skillId))
                return null;

            ImageSource? cached;
            if (IconCache.TryGetValue(skillId, out cached))
                return cached;

            ImageSource? icon = LoadIcon(skillId);
            IconCache[skillId] = icon;
            return icon;
        }

        private static ImageSource? LoadIcon(string skillId)
        {
            string path = Path.Combine(IconDirectory, "camp_skill_" + skillId + ".png");
            if (!File.Exists(path))
                return null;

            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(path, UriKind.Absolute);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            image.Freeze();
            return image;
        }
    }
}
