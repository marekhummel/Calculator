using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Calculator.Converters
{
    internal class BoolToBorderThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (bool)value ? new Thickness(0, 0, 0, 2) : new Thickness(0, 0, 0, 0);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

    }
}
