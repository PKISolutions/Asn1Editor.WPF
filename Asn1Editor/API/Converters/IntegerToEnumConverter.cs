using System;
using System.Globalization;
using System.Windows.Data;

namespace SysadminsLV.Asn1Editor.API.Converters;
class IntegerToEnumConverter : IValueConverter {
    public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture) {
        if (value is Enum) {
            return System.Convert.ToInt32(value);
        }
        return value;
    }
    public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture) {
        if (value is Int32) {
            return Enum.Parse(targetType, value.ToString());
        }
        return value;
    }
}
