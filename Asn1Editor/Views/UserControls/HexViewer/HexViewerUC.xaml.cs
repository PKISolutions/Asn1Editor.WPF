using System;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using SysadminsLV.Asn1Editor.API.Utils;
using SysadminsLV.Asn1Editor.Properties;
using SysadminsLV.Asn1Parser;

namespace SysadminsLV.Asn1Editor.Views.UserControls.HexViewer {
    /// <summary>
    /// Interaction logic for HexViewerUC.xaml
    /// </summary>
    partial class HexViewerUC {
        const String masterAddr = "12345678";
        const String masterHex = "123456789012345678901234567890123456789012345678";
        const String masterAscii = "1234567890123456";

        TextRange[] ranges;
        public HexViewerUC() {
            InitializeComponent();
            ranges = new TextRange[3];
            calculateWidths();
            Settings.Default.PropertyChanged += (Sender, Args) => {
                if (Args.PropertyName == nameof(Settings.FontSize)) {
                    calculateWidths();
                }
            };
        }
        public static readonly DependencyProperty DataSourceProperty = DependencyProperty.Register(
            nameof(DataSource),
            typeof(IBinarySource),
            typeof(HexViewerUC),
            new FrameworkPropertyMetadata(OnDataSourcePropertyChanged));
        static void OnDataSourcePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue != null) {
                ((IBinarySource)e.OldValue).CollectionChanged -= ((HexViewerUC)source).OnCollectionChanged;
            }
            if (e.NewValue != null) {
                ((IBinarySource)e.NewValue).CollectionChanged += ((HexViewerUC)source).OnCollectionChanged;
            }
        }
        void OnCollectionChanged(Object o, NotifyCollectionChangedEventArgs e) {
            if (DataSource == null) { return; }
            buildAddress();
            buildHex();
            buildAscii();
        }

        public static readonly DependencyProperty SelectedNodeProperty = DependencyProperty.Register(
            nameof(SelectedNode),
            typeof(IHexAsnNode),
            typeof(HexViewerUC),
            new FrameworkPropertyMetadata(OnSelectedNodePropertyChanged));
        static void OnSelectedNodePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var ctrl = (HexViewerUC)sender;
            if (e.NewValue == null) {
                TextUtility.ResetColors(ctrl.ranges);
                return;
            }
            var treeNode = (IHexAsnNode)e.NewValue;
            TextUtility.ResetColors(ctrl.ranges);
            ctrl.ranges = TextUtility.GetTextPointers(treeNode, ctrl.HexRawPane, ctrl.Scroller);
            TextUtility.Colorize(ctrl.ranges);
        }

        public IBinarySource DataSource {
            get => (IBinarySource)GetValue(DataSourceProperty);
            set => SetValue(DataSourceProperty, value);
        }
        public IHexAsnNode SelectedNode {
            get => (IHexAsnNode)GetValue(SelectedNodeProperty);
            set => SetValue(SelectedNodeProperty, value);
        }

        void calculateWidths() {
            HexAddrHeaderRtb.Width = Tools.MeasureString(masterAddr, Settings.Default.FontSize, false);
            HexRawHeaderRtb.Width = Tools.MeasureString(masterHex, Settings.Default.FontSize, false);
            HexAsciiHeaderRtb.Width = Tools.MeasureString(masterAscii, Settings.Default.FontSize, false);
        }
        void buildAddress() {
            HexAddressPane.Document = new FlowDocument();
            var addressParagraph = new Paragraph();
            foreach (Int32 row in Enumerable.Range(0, (Int32)Math.Ceiling((Double)DataSource.RawData.Count / 16))) {
                addressParagraph.Inlines.Add(new Run($"{row * 16:X8}" + Environment.NewLine));
            }
            HexAddressPane.Document.Blocks.Add(addressParagraph);
        }
        void buildHex() {
            HexRawPane.Document = new FlowDocument();
            var hexParagraph = new Paragraph();
            hexParagraph.Inlines.Add(new Run(AsnFormatter.BinaryToString(DataSource.RawData.ToArray(), EncodingType.Hex).ToUpper()));
            HexRawPane.Document.Blocks.Add(hexParagraph);
        }
        void buildAscii() {
            HexAsciiPane.Document = new FlowDocument();
            var asciiParagraph = new Paragraph();
            var SB = new StringBuilder();
            for (var index = 0; index < DataSource.RawData.Count; index++) {
                Char c = DataSource.RawData[index] < 32 || DataSource.RawData[index] > 126
                    ? '.'
                    : (Char)DataSource.RawData[index];
                if (index != 0 && index % 16 == 0) {
                    SB.Append(Environment.NewLine);
                }
                SB.Append(c);
            }
            asciiParagraph.Inlines.Add(new Run(SB + Environment.NewLine));
            HexAsciiPane.Document.Blocks.Add(asciiParagraph);
        }
    }
}
