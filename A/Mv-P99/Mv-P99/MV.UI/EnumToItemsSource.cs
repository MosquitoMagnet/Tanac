using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;

namespace Mv.Ui.Core
{
    public class EnumToItemsSource : MarkupExtension
    {
        private readonly Type _type;

        public EnumToItemsSource(Type type)
        {
            _type = type;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Enum.GetValues(_type)
                .Cast<object>()
                .Select(e => new { Value = (int)e, DisplayName = e.ToString() });
        }
    }
    public static class EnumHelper
    {
        private static T GetEnumAttribute<T>(Enum source) where T : Attribute
        {
            Type type = source.GetType();
            var sourceName = Enum.GetName(type, source);
            FieldInfo field = type.GetField(sourceName);
            object[] attributes = field.GetCustomAttributes(typeof(T), false);
            foreach (var o in attributes)
            {
                if (o is T)
                    return (T)o;
            }
            return null;
        }

        public static string GetDescription(Enum source)
        {
            var str = GetEnumAttribute<DescriptionAttribute>(source);
            return str == null ? null : str.Description;
        }

    }

    [TypeConverter(typeof(EnumDefaultValueTypeConverter))]
    public enum RequestTypeValue
    {
        [Description("zh-CN"), DefaultValue("Chinese")]
        zhCN,
        [Description("en-US"), DefaultValue("English")]
        enUS,
        [Description("ja-JP"), DefaultValue("Japanese")]
        jaJP
    }
  

    public class EnumDefaultValueTypeConverter : EnumConverter
    {
        public EnumDefaultValueTypeConverter(Type type) : base(type)
        {
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                if (value != null)
                {
                    FieldInfo fi = value.GetType().GetField(value.ToString());
                    if (fi != null)
                    {
                        var attributes = (DefaultValueAttribute[])fi.GetCustomAttributes(typeof(DefaultValueAttribute), false);
                        return ((attributes.Length > 0) && (!string.IsNullOrEmpty(attributes[0].Value.ToString()))) ? attributes[0].Value : value.ToString();
                    }
                }

                return string.Empty;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
