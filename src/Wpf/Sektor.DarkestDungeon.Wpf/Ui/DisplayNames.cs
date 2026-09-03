using System;
using System.Linq;

namespace Sektor.DarkestDungeon.Wpf.Ui
{
    /// <summary>Presentation helpers deriving display labels from string ids.</summary>
    public static class DisplayNames
    {
        /// <summary>Converts a snake_case class id to a readable title ("plague_doctor" → "Plague Doctor").</summary>
        /// <param name="classId">The raw class id.</param>
        /// <returns>The readable class name.</returns>
        public static string Class(string classId)
        {
            return Title(classId);
        }

        /// <summary>Converts a snake_case id to a readable title ("bleed_debuff_1" → "Bleed Debuff 1").</summary>
        /// <param name="id">The raw id.</param>
        /// <returns>The readable title.</returns>
        public static string Title(string id)
        {
            if (string.IsNullOrEmpty(id))
                return id;

            var parts = id.Split('_');
            return string.Join(" ", parts.Select(p => p.Length > 0 ? char.ToUpper(p[0]) + p.Substring(1) : p));
        }
    }
}