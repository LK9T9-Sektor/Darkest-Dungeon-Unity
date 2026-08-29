using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Sektor.DarkestDungeon.Wpf.Converters
{
    /// <summary>Converts an empty string (or null) to <see cref="Visibility.Collapsed"/>.</summary>
    public sealed class StringToVisibilityConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility.Visible ? string.Empty : null!;
        }
    }
}
