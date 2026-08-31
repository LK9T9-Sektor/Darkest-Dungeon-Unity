using System;
using System.Windows;

using NUnit.Framework;

using Sektor.DarkestDungeon.Wpf.Ui;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Tests for the pure geometry of the hover target arrowhead.</summary>
    [TestFixture]
    public class TargetArrowMathTests
    {
        [Test]
        public void ArrowHead_HorizontalLine_PointsBehindTheTip()
        {
            var points = TargetArrowMath.ArrowHead(new Point(100, 0), new Point(0, 0), 14, 7);

            Assert.That(points.Length, Is.EqualTo(3));
            Assert.That(points[0], Is.EqualTo(new Point(100, 0)), "The tip is the target end of the line.");
            Assert.That(points[1], Is.EqualTo(new Point(86, 7)));
            Assert.That(points[2], Is.EqualTo(new Point(86, -7)));
        }

        [Test]
        public void ArrowHead_VerticalLine_PointsPerpendicular()
        {
            var points = TargetArrowMath.ArrowHead(new Point(0, 100), new Point(0, 0), 14, 7);

            Assert.That(points[1], Is.EqualTo(new Point(-7, 86)));
            Assert.That(points[2], Is.EqualTo(new Point(7, 86)));
        }

        [Test]
        public void ArrowHead_DiagonalLine_FlipsTheWingsAcrossTheDirection()
        {
            var points = TargetArrowMath.ArrowHead(new Point(10, 10), new Point(0, 0), 5, 2);

            double nx = 1 / Math.Sqrt(2);
            double ny = 1 / Math.Sqrt(2);
            double baseX = 10 - nx * 5;
            double baseY = 10 - ny * 5;

            Assert.That(points[0].X, Is.EqualTo(10).Within(0.0001));
            Assert.That(points[0].Y, Is.EqualTo(10).Within(0.0001));
            Assert.That(points[1].X, Is.EqualTo(baseX - ny * 2).Within(0.0001));
            Assert.That(points[1].Y, Is.EqualTo(baseY + nx * 2).Within(0.0001));
            Assert.That(points[2].X, Is.EqualTo(baseX + ny * 2).Within(0.0001));
            Assert.That(points[2].Y, Is.EqualTo(baseY - nx * 2).Within(0.0001));
        }

        [Test]
        public void ArrowHead_ZeroLength_AllPointsCollapseOntoTheTip()
        {
            var points = TargetArrowMath.ArrowHead(new Point(5, 5), new Point(5, 5), 14, 7);

            Assert.That(points[0], Is.EqualTo(new Point(5, 5)));
            Assert.That(points[1], Is.EqualTo(new Point(5, 5)));
            Assert.That(points[2], Is.EqualTo(new Point(5, 5)));
        }
    }
}