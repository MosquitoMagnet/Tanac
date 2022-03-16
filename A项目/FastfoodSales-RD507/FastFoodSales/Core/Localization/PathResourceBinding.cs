using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace DAQ.Core.Localization
{
    public class PathResourceBinding : Binding
    {
        private class PathOfLangConverter : IValueConverter
        {
            private const char pot = '.';

            public static readonly PathOfLangConverter Current = new PathOfLangConverter();

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value == null)
                {
                    return value;
                }

                IEnumerable<string> values = from p in ((string)value).Split('.')
                                             select AppLocalizationService.GetLang(p);
                return string.Join(".", values);
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public PathResourceBinding()
        {
            base.Converter = PathOfLangConverter.Current;
        }

        public PathResourceBinding(string path)
            : this()
        {
            base.Path = new PropertyPath(path);
        }
    }
}
