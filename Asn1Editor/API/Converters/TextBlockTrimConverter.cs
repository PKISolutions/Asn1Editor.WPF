using System;
using System.Globalization;
using System.Windows.Data;
using SysadminsLV.Asn1Editor.Properties;

namespace SysadminsLV.Asn1Editor.API.Converters; 

class TextBlockTrimConverter : IValueConverter {
    public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture) {
        if (value == null) { return String.Empty; }
        String retValue = value.ToString();
        if (retValue.Length > Settings.Default.MaxTreeViewLength) {
            retValue = retValue.Substring(0, Settings.Default.MaxTreeViewLength) + "...";
        }
        return retValue;

    }
    public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture) {
        return null;
    }
}