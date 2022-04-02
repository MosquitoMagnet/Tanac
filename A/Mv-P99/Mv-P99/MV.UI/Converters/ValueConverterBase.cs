using Mv.Core.Extensions;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Mv.Ui.Converters
{
    public abstract class ValueConverterBase<TSource, TTarget, TParameter> : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value.CastTo<TSource>(), parameter.CastTo<TParameter>());
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertBack(value.CastTo<TTarget>(), parameter.CastTo<TParameter>());
        }

        protected virtual TTarget ConvertNonNullValue(TSource value, TParameter parameter) => throw new NotSupportedException();

        protected virtual TTarget Convert(TSource value, TParameter parameter)
        {
            return value != null ? ConvertNonNullValue(value, parameter) : default;
        }

        protected virtual TSource ConvertBack(TTarget value, TParameter parameter) => throw new NotSupportedException();
    }

    public abstract class ValueConverterBase<TSource, TTarget> : ValueConverterBase<TSource, TTarget, object>
    {
        protected sealed override TTarget Convert(TSource value, object parameter) => Convert(value);

        protected sealed override TTarget ConvertNonNullValue(TSource value, object parameter) => throw new NotSupportedException();

        protected sealed override TSource ConvertBack(TTarget value, object parameter) => ConvertBack(value);

        protected virtual TTarget ConvertNonNullValue(TSource value) => throw new NotSupportedException();

        protected virtual TTarget Convert(TSource value) => value != null ? ConvertNonNullValue(value) : default;

        protected virtual TSource ConvertBack(TTarget value) => throw new NotSupportedException();
    }

    public class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string EnumString;
            try
            {
                EnumString = Enum.GetName((value.GetType()), value);
                return EnumString;
            }
            catch
            {
                return string.Empty;
            }
        }

        // No need to implement converting back on a one-way binding 
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NewLineConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = string.Empty;

            if (!string.IsNullOrEmpty(value as string))
            {
                s = value.ToString();

                if (s.Contains("\\r\\n"))
                    s = s.Replace("\\r\\n", Environment.NewLine);

                if (s.Contains("\\n"))
                    s = s.Replace("\\n", Environment.NewLine);

                if (s.Contains("&#x0a;&#x0d;"))
                    s = s.Replace("&#x0a;&#x0d;", Environment.NewLine);

                if (s.Contains("&#x0a;"))
                    s = s.Replace("&#x0a;", Environment.NewLine);

                if (s.Contains("&#x0d;"))
                    s = s.Replace("&#x0d;", Environment.NewLine);

                if (s.Contains("&#10;&#13;"))
                    s = s.Replace("&#10;&#13;", Environment.NewLine);

                if (s.Contains("&#10;"))
                    s = s.Replace("&#10;", Environment.NewLine);

                if (s.Contains("&#13;"))
                    s = s.Replace("&#13;", Environment.NewLine);

                if (s.Contains("<br />"))
                    s = s.Replace("<br />", Environment.NewLine);

                if (s.Contains("<LineBreak />"))
                    s = s.Replace("<LineBreak />", Environment.NewLine);
            }

            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
