using System;
using System.Globalization;
using System.Windows.Data;

namespace PL
{
    public class NullableDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;
            if (value is double d) return d.ToString(CultureInfo.InvariantCulture);
            if (value is double?) return (value as double?)?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value?.ToString();
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return d;
            return null;
        }
    }
}
