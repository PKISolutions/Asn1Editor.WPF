using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace SysadminsLV.Asn1Editor.API.Utils {
    static class Tools {
        public static MessageBoxResult MsgBox(String header, String message, MessageBoxImage image = MessageBoxImage.Error, MessageBoxButton button = MessageBoxButton.OK) {
            WindowCollection windows = Application.Current.Windows;
            Window           hwnd    = null;
            if (windows.Count > 0) {
                hwnd = windows[windows.Count - 1];
            }
            return hwnd == null
                ? MessageBox.Show(message, header, button, image)
                : MessageBox.Show(hwnd, message, header, button, image);
        }
        public static Double MeasureString(String str, Int32 size, Boolean includeScrollbars) {
            var formattedText = new FormattedText(
                str,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Consolas"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                size,
                Brushes.Black);
            return includeScrollbars
                ? formattedText.Width + SystemParameters.VerticalScrollBarWidth + 12
                : formattedText.Width + 12;
        }
    }
}
