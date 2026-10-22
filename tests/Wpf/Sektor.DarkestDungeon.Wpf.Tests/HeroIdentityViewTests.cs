using System.Threading;
using System.Windows;
using System.Windows.Controls;

using NUnit.Framework;

using Sektor.DarkestDungeon.Wpf.Views;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Verifies the reusable hero identity block switches between display and edit modes.</summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class HeroIdentityViewTests
    {
        /// <summary>Edit mode swaps the name label for a length-limited text box.</summary>
        [Test]
        public void IsEditing_TogglesTheNameEditor()
        {
            EnsureApplication();
            var view = new HeroIdentityView();
            var display = (StackPanel)view.FindName("DisplayPanel");
            var editor = (StackPanel)view.FindName("EditPanel");
            var nameBox = (TextBox)view.FindName("NameBox");
            var nameHint = (TextBlock)view.FindName("NameHint");

            Assert.That(display.Visibility, Is.EqualTo(Visibility.Visible));
            Assert.That(editor.Visibility, Is.EqualTo(Visibility.Collapsed));
            Assert.That(nameBox.MaxLength, Is.EqualTo(24));
            Assert.That(nameHint.Text, Has.Length.EqualTo(24));

            view.IsEditing = true;

            Assert.That(display.Visibility, Is.EqualTo(Visibility.Collapsed));
            Assert.That(editor.Visibility, Is.EqualTo(Visibility.Visible));
        }

        private static void EnsureApplication()
        {
            if (Application.Current != null)
                return;

            var app = new App();
            app.InitializeComponent();
        }
    }
}
