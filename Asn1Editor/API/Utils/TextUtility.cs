using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.Controls;

namespace SysadminsLV.Asn1Editor.API.Utils {
    static class TextUtility {
        static Int32 getOffset(Int32 offset) {
            Int32 line = (Int32)Math.Floor((Double)offset / 16);
            return (offset - 16 * line) * 3 + 50 * line + 2;
        }
        static TextRange[] GetRanges(IList<TextPointer> pointers) {
            TextRange[] ranges = new TextRange[3];
            ranges[0] = new TextRange(pointers[0], pointers[1]);
            ranges[1] = new TextRange(pointers[2], pointers[3]);
            ranges[2] = new TextRange(pointers[4], pointers[5]);
            return ranges;
        }
        static void ScrollToPosition(TextPointer startPointer, BindableRichTextBox rtb, ScrollViewer scroller) {
            rtb.CaretPosition = startPointer;
            scroller.ScrollToVerticalOffset(rtb.FontSize * (rtb.CurrentLine - 1));
        }

        public static TextRange[] GetTextPointers(Asn1Lite treeNode, BindableRichTextBox rtb, ScrollViewer scroller) {
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
            //// payload
            pointers[4] = rtb.Document.ContentStart.GetPositionAtOffset(
                getOffset(treeNode.PayloadStartOffset)
            );
            pointers[5] = rtb.Document.ContentStart.GetPositionAtOffset(
                getOffset(treeNode.PayloadStartOffset + treeNode.PayloadLength)
            );
            ScrollToPosition(pointers[0], rtb, scroller);
            return GetRanges(pointers);
        }
        public static void Colorize(TextRange[] ranges) {
            foreach (TextRange range in ranges) {
                range.ApplyPropertyValue(TextElement.FontWeightProperty,FontWeights.Bold);
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
    }
}
