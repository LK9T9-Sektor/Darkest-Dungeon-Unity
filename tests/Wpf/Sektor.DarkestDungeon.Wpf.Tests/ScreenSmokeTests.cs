namespace Sektor.DarkestDungeon.Wpf.Tests
{
    using System.Threading;

    using NUnit.Framework;

    using Sektor.DarkestDungeon.Wpf.Views;

    /// <summary>Loads every screen XAML against the real application resources (parse smoke test).</summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class ScreenSmokeTests
    {
        /// <summary>Instantiating each view forces XAML parsing and static-resource resolution.</summary>
        [Test]
        public void AllScreens_LoadTheirXaml()
        {
            var app = new App();
            app.InitializeComponent();

            Assert.DoesNotThrow(() => _ = new MainMenuView());
            Assert.DoesNotThrow(() => _ = new DuelLobbyView());
            Assert.DoesNotThrow(() => _ = new SinglePlayerLobbyView());
            Assert.DoesNotThrow(() => _ = new DuelBattleView());
            Assert.DoesNotThrow(() => _ = new HeroStatsView());
            Assert.DoesNotThrow(() => _ = new HeroStatsView { ShowFullDetails = true });
            Assert.DoesNotThrow(() => _ = new ScreenHeaderView { Title = "T", CloseCommand = null });
        }
    }
}