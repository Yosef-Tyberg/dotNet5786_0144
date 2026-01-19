using System;
using System.Globalization;
using System.Windows.Data;

namespace PL
{
    public class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is DateTime dt
                ? dt.ToString("yyyy-MM-dd HH:mm:ss")
                : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            DateTime.TryParse(value?.ToString(), out var dt)
                ? dt
                : DateTime.Now;
    }
}
