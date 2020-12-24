using System;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using SysadminsLV.Asn1Parser;
using SysadminsLV.WPF.OfficeTheme.Controls;

namespace Asn1Editor.Wpf.Controls {
    /// <summary>
    /// Interaction logic for HexViewerUC.xaml
    /// </summary>
    partial class AsnHexViewer {
        const String masterAddr = "12345678";
        const String masterHex = "123456789012345678901234567890123456789012345678";
        const String masterAscii = "1234567890123456";

        readonly BindableRichTextBox[] panes;

        Boolean scrollLocked;
        TextRange[] ranges;
        public AsnHexViewer() {
            InitializeComponent();
            FontSizeProperty.OverrideMetadata(typeof(AsnHexViewer), new FrameworkPropertyMetadata(OnFontSizeChanged));
            ranges = new TextRange[3];
            calculateWidths();

            panes = new[] { HexAddressPane, HexRawPane, HexAsciiPane };
        }


        static void OnFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((AsnHexViewer)d).calculateWidths();
        }


        #region ShowAddressPane

        public static readonly DependencyProperty ShowAddressPaneProperty = DependencyProperty.Register(
            nameof(ShowAddressPane),
            typeof(Boolean),
            typeof(AsnHexViewer),
            new PropertyMetadata(default(Boolean)));

        public Boolean ShowAddressPane {
            get => (Boolean)GetValue(ShowAddressPaneProperty);
            set => SetValue(ShowAddressPaneProperty, value);
        }

        #endregion

        #region ShowAsciiPane

        public static readonly DependencyProperty ShowAsciiPaneProperty = DependencyProperty.Register(
            nameof(ShowAsciiPane),
            typeof(Boolean),
            typeof(AsnHexViewer),
            new PropertyMetadata(default(Boolean)));

        public Boolean ShowAsciiPane {
            get => (Boolean)GetValue(ShowAsciiPaneProperty);
            set => SetValue(ShowAsciiPaneProperty, value);
        }

        #endregion

        #region DataSource

        public static readonly DependencyProperty DataSourceProperty = DependencyProperty.Register(
            nameof(DataSource),
            typeof(IBinarySource),
            typeof(AsnHexViewer),
            new FrameworkPropertyMetadata(OnDataSourcePropertyChanged));
        public IBinarySource DataSource {
            get => (IBinarySource)GetValue(DataSourceProperty);
            set => SetValue(DataSourceProperty, value);
        }
        static void OnDataSourcePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue != null) {
                ((IBinarySource)e.OldValue).CollectionChanged -= ((AsnHexViewer)source).OnCollectionChanged;
            }
            if (e.NewValue != null) {
                ((IBinarySource)e.NewValue).CollectionChanged += ((AsnHexViewer)source).OnCollectionChanged;
            }
        }
        void OnCollectionChanged(Object o, NotifyCollectionChangedEventArgs e) {
            if (DataSource == null) { return; }
            buildAddress();
            buildHex();
            buildAscii();
        }

        #endregion

        #region SelectedNode

        public static readonly DependencyProperty SelectedNodeProperty = DependencyProperty.Register(
            nameof(SelectedNode),
            typeof(IHexAsnNode),
            typeof(AsnHexViewer),
            new FrameworkPropertyMetadata(OnSelectedNodePropertyChanged));
        static void OnSelectedNodePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var ctrl = (AsnHexViewer)sender;
            if (e.NewValue == null) {
                TextUtility.ResetColors(ctrl.ranges);
                return;
            }
            var treeNode = (IHexAsnNode)e.NewValue;
            TextUtility.ResetColors(ctrl.ranges);
            ctrl.ranges = TextUtility.GetSelectionPointers(treeNode, ctrl.HexRawPane);
            TextUtility.Colorize(ctrl.ranges);
            ctrl.HexRawPane.CaretPosition = ctrl.ranges[0].Start;
            ctrl.scrollPanes(null);
        }


        public IHexAsnNode SelectedNode {
            get => (IHexAsnNode)GetValue(SelectedNodeProperty);
            set => SetValue(SelectedNodeProperty, value);
        }

        #endregion

        void calculateWidths() {
            HexAddrHeaderRtb.Width = TextUtility.MeasureStringWidth(masterAddr, FontSize, false);
            HexRawHeaderRtb.Width = TextUtility.MeasureStringWidth(masterHex, FontSize, false);
            HexAsciiHeaderRtb.Width = TextUtility.MeasureStringWidth(masterAscii, FontSize, false);
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
            for (Int32 index = 0; index < DataSource.RawData.Count; index++) {
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
        void onRtbScrollChanged(Object sender, ScrollChangedEventArgs e) {
            if (scrollLocked) {
                return;
            }
            scrollLocked = true;
            var scrollViewer = (ScrollViewer)e.OriginalSource;
            Scroller.Maximum = scrollViewer.ScrollableHeight;
            Scroller.ViewportSize = scrollViewer.ViewportHeight;
            Scroller.Value = scrollViewer.VerticalOffset;
            scrollPanes(scrollViewer.VerticalOffset);
            scrollLocked = false;
        }
        void scrollPanes(Double? newValue) {
            Double vOffset = newValue ?? HexRawPane.FontSize * HexRawPane.FontFamily.LineSpacing * (HexRawPane.CurrentLine - 1);
            for (Int32 i = 0; i < panes.Length; i++) {
                if (i > 0) {
                    // do not fire re-scroll for the rest of RTBs
                    scrollLocked = true;
                }
                panes[i].ScrollToVerticalOffset(vOffset);
            }
            scrollLocked = false;
        }
        void onScrollerScroll(Object sender, ScrollEventArgs e) {
            Double smallStep = 48;
            const Double bigStep = 256;
            Double finalValue = e.NewValue;

            switch (e.ScrollEventType) {
                case ScrollEventType.LargeDecrement:
                    finalValue = e.NewValue - bigStep < Scroller.Minimum
                        ? Scroller.Minimum
                        : e.NewValue - bigStep;
                    break;
                case ScrollEventType.LargeIncrement:
                    finalValue = e.NewValue + bigStep > Scroller.Maximum
                        ? Scroller.Maximum
                        : e.NewValue + bigStep;
                    break;
                case ScrollEventType.SmallDecrement:
                    finalValue = e.NewValue - smallStep < Scroller.Minimum
                        ? Scroller.Minimum
                        : e.NewValue - smallStep;
                    break;
                case ScrollEventType.SmallIncrement:
                    finalValue = e.NewValue + smallStep > Scroller.Maximum
                        ? Scroller.Maximum
                        : e.NewValue + smallStep;
                    break;
            }

            Scroller.Value = finalValue;
            scrollPanes(finalValue);
            e.Handled = true;
        }
    }
}
