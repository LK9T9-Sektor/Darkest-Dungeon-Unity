using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Sektor.DarkestDungeon.Wpf.Converters
{
    /// <summary>Converts an empty string (or null) to <see cref="Visibility.Collapsed"/>. Pass
    /// <c>Invert</c> as the converter parameter to show the element for an empty value instead.</summary>
    public sealed class StringToVisibilityConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            bool hasText = !string.IsNullOrEmpty(value as string);
            bool invert = string.Equals(parameter as string, "Invert", System.StringComparison.OrdinalIgnoreCase);
            return hasText != invert ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility.Visible ? string.Empty : null!;
        }
    }
}
