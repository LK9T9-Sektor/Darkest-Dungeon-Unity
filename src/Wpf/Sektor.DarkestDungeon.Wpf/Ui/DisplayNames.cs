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
            if (string.IsNullOrEmpty(classId))
                return classId;

            var parts = classId.Split('_');
            return string.Join(" ", parts.Select(p => p.Length > 0 ? char.ToUpper(p[0]) + p.Substring(1) : p));
        }
    }
}