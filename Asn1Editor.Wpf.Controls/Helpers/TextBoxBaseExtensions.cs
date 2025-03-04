using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Asn1Editor.Wpf.Controls.Helpers;
static class TextBoxBaseExtensions {
    public static void MeasureStringWidth(this TextBoxBase textBoxBase, String str, Double size, Boolean includeScrollbars) {
        FormattedText formattedText = new FormattedText(
            str,
            CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight,
            new Typeface(new FontFamily("Consolas"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
            size, Brushes.Black,
            VisualTreeHelper.GetDpi(textBoxBase).PixelsPerDip);

        textBoxBase.Width = includeScrollbars
            ? formattedText.Width + SystemParameters.VerticalScrollBarWidth + 12
            : formattedText.Width + 12;
    }
}
