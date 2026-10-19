using NUnit.Framework;

using Sektor.DarkestDungeon.Wpf.Navigation;
using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Verifies the Spine demo screen state.</summary>
    [TestFixture]
    public class SpineDemoViewModelTests
    {
        /// <summary>The screenshot result reports the saved file name.</summary>
        [Test]
        public void ScreenshotSavedCommand_WithPath_ShowsFileName()
        {
            var viewModel = new SpineDemoViewModel(new FakeNavigationService());

            viewModel.ScreenshotSavedCommand.Execute(@"C:\shots\screenshot-2026-10-19_12-00-00-000.png");

            Assert.That(viewModel.ScreenshotStatus, Is.EqualTo("Saved: screenshot-2026-10-19_12-00-00-000.png"));
        }

        /// <summary>A failed capture reports the failure instead of a path.</summary>
        [Test]
        public void ScreenshotSavedCommand_WithoutPath_ReportsFailure()
        {
            var viewModel = new SpineDemoViewModel(new FakeNavigationService());

            viewModel.ScreenshotSavedCommand.Execute(null);

            Assert.That(viewModel.ScreenshotStatus, Is.EqualTo("Screenshot failed"));
        }

        private sealed class FakeNavigationService : INavigationService
        {
            public void NavigateTo(object screen)
            {
            }

            public void GoHome()
            {
            }
        }
    }
}
