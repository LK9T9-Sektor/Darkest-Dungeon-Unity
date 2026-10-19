using System;
using System.Windows;
using System.Windows.Input;

using Microsoft.Xaml.Behaviors;

using Sektor.DarkestDungeon.Wpf.Ui;

namespace Sektor.DarkestDungeon.Wpf.Behaviors
{
    /// <summary>Captures the host window into a timestamped PNG and reports the result to a command.</summary>
    public sealed class SaveScreenshotAction : TriggerAction<FrameworkElement>
    {
        /// <summary>Identifies the <see cref="SavedCommand"/> dependency property.</summary>
        public static readonly DependencyProperty SavedCommandProperty =
            DependencyProperty.Register(
                nameof(SavedCommand),
                typeof(ICommand),
                typeof(SaveScreenshotAction),
                new PropertyMetadata(null));

        /// <summary>Gets or sets the command invoked with the saved file path, or null when the capture failed.</summary>
        public ICommand? SavedCommand
        {
            get => (ICommand?)GetValue(SavedCommandProperty);
            set => SetValue(SavedCommandProperty, value);
        }

        /// <inheritdoc />
        protected override void Invoke(object parameter)
        {
            string? path = null;
            Window? window = AssociatedObject != null ? Window.GetWindow(AssociatedObject) : null;
            if (window != null)
            {
                try
                {
                    path = WindowScreenshot.Capture(window);
                }
                catch (Exception)
                {
                    path = null;
                }
            }

            ICommand? command = SavedCommand;
            if (command != null && command.CanExecute(path))
                command.Execute(path);
        }
    }
}
