using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Calculator.Converters
{
    internal class BoolToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (bool)value ? Brushes.DarkGray : Brushes.Black;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
