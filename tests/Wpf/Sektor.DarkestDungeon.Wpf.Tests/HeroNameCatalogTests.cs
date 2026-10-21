using System;

using NUnit.Framework;

using Sektor.DarkestDungeon.Wpf.Data;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Verifies the bundled English hero name pool.</summary>
    [TestFixture]
    public class HeroNameCatalogTests
    {
        /// <summary>The Unity Names.xml pool is delivered and yields non-empty random names.</summary>
        [Test]
        public void NamePool_IsLoaded()
        {
            Assert.That(HeroNameCatalog.All, Is.Not.Empty);
            Assert.That(HeroNameCatalog.All.Count, Is.GreaterThan(500));
            Assert.That(HeroNameCatalog.RandomName(new Random(1)), Is.Not.Null.And.Not.Empty);
        }
    }
}
