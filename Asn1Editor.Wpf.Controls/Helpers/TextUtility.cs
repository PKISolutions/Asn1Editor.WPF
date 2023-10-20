using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using SysadminsLV.WPF.OfficeTheme.Controls;

namespace Asn1Editor.Wpf.Controls.Helpers; 

public static class TextUtility {
    static Int32 getOffset(Int32 offset) {
        Int32 line = (Int32)Math.Floor((Double)offset / 16);
        return (offset - 16 * line) * 3 + 50 * line + 2;
    }
    public static TextRange[] GetRanges(IList<TextPointer> pointers) {
        TextRange[] ranges = new TextRange[3];
        ranges[0] = new TextRange(pointers[0], pointers[1]);
        ranges[1] = new TextRange(pointers[2], pointers[3]);
        ranges[2] = new TextRange(pointers[4], pointers[5]);
        return ranges;
    }

    public static TextRange[] GetSelectionPointers(IHexAsnNode treeNode, BindableRichTextBox rtb) {
        TextPointer[] pointers = new TextPointer[6];
        // tag
        pointers[0] = rtb.Document.ContentStart.GetPositionAtOffset(
            getOffset(treeNode.Offset)
        );
        pointers[1] = rtb.Document.ContentStart.GetPositionAtOffset(
            getOffset(treeNode.Offset + 1)
        );
        // length bytes
        pointers[2] = rtb.Document.ContentStart.GetPositionAtOffset(
            getOffset(treeNode.Offset + 1)
        );
        pointers[3] = rtb.Document.ContentStart.GetPositionAtOffset(
            getOffset(treeNode.PayloadStartOffset)
        );
        // payload
        pointers[4] = rtb.Document.ContentStart.GetPositionAtOffset(
            getOffset(treeNode.PayloadStartOffset)
        );
        pointers[5] = rtb.Document.ContentStart.GetPositionAtOffset(
            getOffset(treeNode.PayloadStartOffset + treeNode.PayloadLength)
        );

        return GetRanges(pointers);
    }
    public static void Colorize(TextRange[] ranges) {
        foreach (TextRange range in ranges) {
            range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
        }
        ranges[0].ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
        ranges[1].ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Green);
        ranges[2].ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
    }
    public static void ResetColors(IEnumerable<TextRange> ranges) {
        foreach (TextRange range in ranges.Where(range => range != null)) {
            range.ClearAllProperties();
        }
    }
    public static Double MeasureStringWidth(String str, Double size, Boolean includeScrollbars) {
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