using System;
using System.Globalization;
using System.Windows.Data;

namespace HappyTetris.Converters
{
    public class RatioConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue && double.TryParse(parameter?.ToString(), out double ratio))
            {
                return doubleValue * ratio;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
