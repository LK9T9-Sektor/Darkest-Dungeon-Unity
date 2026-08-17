using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Sektor.DarkestDungeon.Wpf.Converters
{
    /// <summary>Converts a boolean to <see cref="Visibility"/> with the values flipped.</summary>
    public sealed class InverseBoolToVisibilityConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = value is true;
            return flag ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility.Visible == false;
        }
    }
}
