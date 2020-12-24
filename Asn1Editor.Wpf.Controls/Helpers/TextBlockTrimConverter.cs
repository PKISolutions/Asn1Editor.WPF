using System;
using System.Globalization;
using System.Windows.Data;

namespace Asn1Editor.Wpf.Controls.Helpers {
    public class TextBlockTrimConverter : IMultiValueConverter {
        public Object Convert(Object[] values, Type targetType, Object parameter, CultureInfo culture) {
            if (values == null || values.Length < 2 || !(values[1] is Int32 maxLength)) {
                return String.Empty;
            }

            String retValue = values[0].ToString();
            if (retValue.Length > maxLength) {
                retValue = retValue.Substring(0, maxLength) + "...";
            }
            return retValue;

        }
        public Object[] ConvertBack(Object value, Type[] targetTypes, Object parameter, CultureInfo culture) {
            return null;
        }
    }
}
