using System.Windows;

using Microsoft.Xaml.Behaviors;

namespace Sektor.DarkestDungeon.Wpf.Behaviors
{
    /// <summary>Marks a routed event as handled so it does not reach ancestor elements.</summary>
    public sealed class MarkEventHandledAction : TriggerAction<FrameworkElement>
    {
        /// <inheritdoc />
        protected override void Invoke(object parameter)
        {
            if (parameter is RoutedEventArgs args)
                args.Handled = true;
        }
    }
}
