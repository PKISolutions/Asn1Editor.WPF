using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.Properties;
using SysadminsLV.Asn1Editor.Views.Windows;
using SysadminsLV.Asn1Parser;
using Unity;

namespace SysadminsLV.Asn1Editor.API.ViewModel {
    static class StaticCommands {
        static Converter converter;
        static Boolean converterClosed = true;
        static TextViewer textViewer;
        static Boolean textViewerClosed = true;

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
            PrintDialog printDialog = new PrintDialog();
            if (!printDialog.ShowDialog().GetValueOrDefault()) { return; }
            FlowDocument flowDocument = new FlowDocument {
                PageHeight = printDialog.PrintableAreaHeight,
                PageWidth = printDialog.PrintableAreaWidth,
                PagePadding = new Thickness(25),
                ColumnGap = 0,
            };
            flowDocument.ColumnWidth = flowDocument.PageWidth -
                flowDocument.ColumnGap -
                flowDocument.PagePadding.Left -
                flowDocument.PagePadding.Right;
            Paragraph paragraph = new Paragraph {
                Margin = new Thickness(0),
                FontFamily = new FontFamily("Consolas"),
                FontSize = Settings.Default.FontSize,
            };
            paragraph.Inlines.Add(new Run(text));
            flowDocument.Blocks.Add(paragraph);
            DocumentPaginator paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
            printDialog.PrintDocument(paginator, String.Empty);
        }
        public static void ShowConverter(Object obj) {
            if (!converterClosed) {
                converter.Focus();
                return;
            }
            converterClosed = false;
            converter = new Converter();
            converter.Closed += (Sender, Args) => { converterClosed = true; };
            ((ConverterVM)converter.DataContext).PropertyChanged += OnPropertyChanged;
            ((ConverterVM)converter.DataContext).SetBytes(App.Container.Resolve<IDataSource>().RawData);
            converter.Show();
            converter.Focus();
        }
        public static void ShowText(Object obj) {
            if (obj == null) { return; }
            if (!textViewerClosed) {
                textViewer.Focus();
                return;
            }
            textViewerClosed = false;
            textViewer = App.Container.Resolve<TextViewer>();
            textViewer.Closed += (Sender, Args) => { textViewerClosed = true; };
            ((TextViewerVM)textViewer.DataContext).SetBinding((Asn1TreeNode)obj);
            textViewer.Show();
            textViewer.Focus();
        }
        public static void ClearResources() {
            if (!textViewerClosed) { textViewer.Close(); }
            if (!converterClosed) { converter.Close(); }
        }

        static void OnPropertyChanged(Object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "Decode") {
                App.Container.Resolve<IMainWindowVM>().OpenRaw(((ConverterVM)sender).RawData);
            }
        }
    }
}
