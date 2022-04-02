﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace Mv.Ui.Converters
{
    public class IsLessThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var left = double.Parse(value.ToString());
            var right = double.Parse(parameter.ToString());
            return left < right;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsGreaterThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var left = double.Parse(value.ToString());
            var right = double.Parse(parameter.ToString());
            return left > right;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
