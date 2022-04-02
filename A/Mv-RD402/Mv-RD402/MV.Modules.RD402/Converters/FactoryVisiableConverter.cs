using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Mv.Modules.RD402.Converters
{
    public class FactoryVisiableConverter : IValueConverter
    {
        public string TargetFactory { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                throw new ArgumentNullException($"{nameof(FactoryVisiableConverter)} value can not be null");
            return value.ToString() == TargetFactory ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
