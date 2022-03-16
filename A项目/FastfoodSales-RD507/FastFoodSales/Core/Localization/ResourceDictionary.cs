using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DAQ.Core.Localization
{
    public class ResourceBinding : Binding
    {
        private class LangConverter : IValueConverter
        {
            public static readonly LangConverter Current = new LangConverter();

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value == null)
                {
                    return value;
                }

                return AppLocalizationService.GetLang((string)value);
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public ResourceBinding()
        {
            base.Converter = LangConverter.Current;
        }

        public ResourceBinding(string path)
            : this()
        {
            base.Path = new PropertyPath(path);
        }
    }
}
