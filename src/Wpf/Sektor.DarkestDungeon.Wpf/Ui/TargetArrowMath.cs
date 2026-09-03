using System;
using System.Windows;

namespace Sektor.DarkestDungeon.Wpf.Ui
{
    /// <summary>Pure geometry helpers for the hover target arrow (a straight line plus an arrowhead).</summary>
    public static class TargetArrowMath
    {
        /// <summary>Computes the three points of the arrowhead at the line's target end.</summary>
        /// <param name="tip">The arrow tip (target end of the line).</param>
        /// <param name="tail">The line start (source end).</param>
        /// <param name="length">The arrowhead length along the line direction.</param>
        /// <param name="spread">The half-width of the arrowhead wings (perpendicular offset from the line).</param>
        /// <returns>The tip, left-wing and right-wing points.</returns>
        public static Point[] ArrowHead(Point tip, Point tail, double length, double spread)
        {
            double dx = tip.X - tail.X;
            double dy = tip.Y - tail.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            if (distance <= 0.0001)
                return new[] { tip, tip, tip };

            double nx = dx / distance;
            double ny = dy / distance;
            double px = -ny;
            double py = nx;

            double baseX = tip.X - nx * length;
            double baseY = tip.Y - ny * length;

            return new[]
            {
                tip,
                new Point(baseX + px * spread, baseY + py * spread),
                new Point(baseX - px * spread, baseY - py * spread),
            };
        }
    }
}