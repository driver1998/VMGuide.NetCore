using System;
using System.Reflection;
using System.ComponentModel;
using System.Linq;
using System.Globalization;
using Avalonia.Data.Converters;

namespace app
{
    public sealed class DateTimeOffsetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime)
            {
                return new DateTimeOffset((DateTime)value);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTimeOffset)
            {
                var offset = (DateTimeOffset)value;
                return offset.DateTime;
            }
            else
            {
                throw new NotImplementedException();
            }
            
        }
    }
    
    public sealed class EnumDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum)
            {
                var t = value.GetType();
                var field = t.GetField(value.ToString());
                var desc = field.GetCustomAttributes<DescriptionAttribute>().FirstOrDefault();
                return desc?.Description ?? field.Name;
            }
            else
            {
                return value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public sealed class EnumOptionsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum)
            {
                return value.GetType().GetEnumValues().OfType<Enum>().ToList();
            }
            else
            {
                return value.ToString();
            }
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}