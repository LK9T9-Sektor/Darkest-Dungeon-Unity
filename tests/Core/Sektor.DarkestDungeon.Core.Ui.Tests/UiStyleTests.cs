using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Ui;

namespace Sektor.DarkestDungeon.Core.Ui.Tests
{
    [TestFixture]
    public class UiStyleTests
    {
        [Test]
        public void FontResource_IsNotEmpty()
        {
            Assert.That(UiStyle.FontResource, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void TextSizes_ArePositive()
        {
            Assert.That(UiStyle.Small, Is.GreaterThan(0));
            Assert.That(UiStyle.Body, Is.GreaterThan(0));
            Assert.That(UiStyle.LogBody, Is.GreaterThan(0));
            Assert.That(UiStyle.Title, Is.GreaterThan(0));
            Assert.That(UiStyle.LargeTitle, Is.GreaterThan(0));
            Assert.That(UiStyle.RowLabel, Is.GreaterThan(0));
            Assert.That(UiStyle.Value, Is.GreaterThan(0));
            Assert.That(UiStyle.ReturnButton, Is.GreaterThan(0));
        }

        [Test]
        public void ArgbColor_KeepsChannels()
        {
            ArgbColor color = ArgbColor.FromArgb(242, 115, 97, 51);

            Assert.That(color.A, Is.EqualTo(242));
            Assert.That(color.R, Is.EqualTo(115));
            Assert.That(color.G, Is.EqualTo(97));
            Assert.That(color.B, Is.EqualTo(51));
        }
    }
}
