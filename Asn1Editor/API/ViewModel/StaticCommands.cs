using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.Properties;
using SysadminsLV.Asn1Parser;

namespace SysadminsLV.Asn1Editor.API.ViewModel {
    static class StaticCommands {

        public static void UpdateSettingsDecode(Asn1TreeNode rootNode) {
            foreach (Asn1Lite node in rootNode.Flatten()) {
                node.UpdateView();
            }
        }
        public static void UpdateSettingsInteger(Asn1TreeNode rootNode, IList<Byte> rawData) {
            if (Settings.Default.IntAsInt) {
                foreach (Asn1Lite node in rootNode.Flatten().Where(x => x.Tag == (Byte)Asn1Type.INTEGER)) {
                    Byte[] raw = rawData.Skip(node.PayloadStartOffset).Take(node.PayloadLength).ToArray();
                    node.ExplicitValue = new BigInteger(raw.Reverse().ToArray()).ToString();
                }
            } else {
                foreach (Asn1Lite node in rootNode.Flatten().Where(x => x.Tag == (Byte)Asn1Type.INTEGER)) {
                    Byte[] raw = rawData.Skip(node.PayloadStartOffset).Take(node.PayloadLength).ToArray();
                    node.ExplicitValue = AsnFormatter.BinaryToString(
                        raw,
                        EncodingType.HexRaw,
                        EncodingFormat.NOCRLF
                    );
                }
            }
        }
        public static void Print(String text) {
            var printDialog = new PrintDialog();
            if (!printDialog.ShowDialog().GetValueOrDefault()) { return; }
            var flowDocument = new FlowDocument {
                PageHeight = printDialog.PrintableAreaHeight,
                PageWidth = printDialog.PrintableAreaWidth,
                PagePadding = new Thickness(25),
                ColumnGap = 0,
            };
            flowDocument.ColumnWidth = flowDocument.PageWidth -
                flowDocument.ColumnGap -
                flowDocument.PagePadding.Left -
                flowDocument.PagePadding.Right;
            var paragraph = new Paragraph {
                Margin = new Thickness(0),
                FontFamily = new FontFamily("Consolas"),
                FontSize = Settings.Default.FontSize,
            };
            paragraph.Inlines.Add(new Run(text));
            flowDocument.Blocks.Add(paragraph);
            DocumentPaginator paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
            printDialog.PrintDocument(paginator, String.Empty);
        }
    }
}
