using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Mv.Core.Interfaces;

namespace Mv.Ui.Converters
{
    public class LessThan1000Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            var number = (int)value;
            if (number <= 0) return null;
            if (number < 1000) return number;
            return "999+";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class RoleLimitConverter : IValueConverter
    {
        public MvRole Limit { get; set; } = MvRole.Admin;   
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is MvRole role)) return Visibility.Hidden;
            return (int) role < (int) Limit ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class BoolToVisibilty : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool m)
            {
                return m ? Visibility.Visible : Visibility.Hidden;
            }
            else
                return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
