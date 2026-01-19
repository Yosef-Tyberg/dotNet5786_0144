using System;
using System.Globalization;
using System.Windows.Data;

namespace PL
{
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan ts)
                return ts.ToString();
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return TimeSpan.Zero;
            if (TimeSpan.TryParse(value.ToString(), out var ts)) return ts;
            // fallback: 0
            return TimeSpan.Zero;
        }
    }
}
