using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using NUnit.Framework;

using Sektor.DarkestDungeon.Wpf.Ui;
using Sektor.DarkestDungeon.Wpf.ViewModels;
using Sektor.DarkestDungeon.Wpf.Views;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Verifies the reusable skill row: items, icon size and command forwarding.</summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class AbilityRowViewTests
    {
        /// <summary>The row renders one square per item and forwards the icon size and name flag.</summary>
        [Test]
        public void ItemsSource_RendersConfiguredSkillSquares()
        {
            var view = new AbilityRowView
            {
                ItemsSource = new List<DuelSkillViewModel>
                {
                    new DuelSkillViewModel("smite", "Smite", SkillTone.Attack),
                    new DuelSkillViewModel("zealous_accusation", "Zealous Accusation", SkillTone.Attack),
                },
                IconSize = 32,
                ShowNames = false,
            };

            Layout(view);

            var squares = FindChildren<SkillSquareView>(view).ToList();
            Assert.That(squares.Count, Is.EqualTo(2));
            Assert.That(squares.All(square => square.IconSize == 32), Is.True);
            Assert.That(squares.All(square => square.ShowName == false), Is.True);
        }

        /// <summary>The row forwards the level flag to every skill square.</summary>
        [Test]
        public void ShowLevels_IsForwardedToSkillSquares()
        {
            var view = new AbilityRowView
            {
                ItemsSource = new List<DuelSkillViewModel>
                {
                    new DuelSkillViewModel("smite", "Smite", SkillTone.Attack),
                },
                IconSize = 32,
                ShowNames = false,
                ShowLevels = true,
            };

            Layout(view);

            var square = FindChildren<SkillSquareView>(view).Single();
            Assert.That(square.ShowLevel, Is.True);
        }

        /// <summary>The row forwards its select command to every skill square.</summary>
        [Test]
        public void SelectCommand_IsForwardedToSkillSquares()
        {
            var command = new StubCommand();
            var view = new AbilityRowView
            {
                ItemsSource = new List<DuelSkillViewModel>
                {
                    new DuelSkillViewModel("smite", "Smite", SkillTone.Attack),
                },
                SelectCommand = command,
            };

            Layout(view);

            SkillSquareView square = FindChildren<SkillSquareView>(view).Single();
            Assert.That(square.SelectCommand, Is.SameAs(command));
        }

        private sealed class StubCommand : ICommand
        {
#pragma warning disable CS0067
            public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

            public bool CanExecute(object? parameter) => true;

            public void Execute(object? parameter)
            {
            }
        }

        private static void Layout(FrameworkElement view)
        {
            var size = new Size(800, 200);
            view.Measure(size);
            view.Arrange(new Rect(size));
            view.UpdateLayout();
        }

        private static List<T> FindChildren<T>(DependencyObject parent)
            where T : DependencyObject
        {
            var result = new List<T>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typed)
                    result.Add(typed);
                result.AddRange(FindChildren<T>(child));
            }

            return result;
        }
    }
}
