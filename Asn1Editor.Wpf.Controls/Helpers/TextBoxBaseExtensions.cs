using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Asn1Editor.Wpf.Controls.Helpers;
static class TextBoxBaseExtensions {
    /// <summary>
    /// Sets <see cref="TextBoxBase"/> control's width to a value required to fit requested text string.
    /// </summary>
    /// <param name="textBoxBase">A <see cref="TextBoxBase"/> instance that defines font style.</param>
    /// <param name="str">String to fit.</param>
    public static void MeasureStringWidth(this TextBoxBase textBoxBase, String str) {
        FormattedText formattedText = new FormattedText(
            str,
            CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight,
            new Typeface(textBoxBase.FontFamily, textBoxBase.FontStyle, textBoxBase.FontWeight, textBoxBase.FontStretch),
            textBoxBase.FontSize,
            textBoxBase.Foreground,
            VisualTreeHelper.GetDpi(textBoxBase).PixelsPerDip);

        textBoxBase.Width = textBoxBase.AcceptsReturn
            ? formattedText.Width + SystemParameters.VerticalScrollBarWidth + 12
            : formattedText.Width + 12;
    }
}