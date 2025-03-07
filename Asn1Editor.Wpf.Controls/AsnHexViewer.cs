using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using Asn1Editor.Wpf.Controls.Helpers;
using SysadminsLV.Asn1Parser;
using SysadminsLV.WPF.OfficeTheme.Controls;

namespace Asn1Editor.Wpf.Controls;

public class AsnHexViewer : Control {
    const String masterAddr = "12345678";
    const String masterHex = "123456789012345678901234567890123456789012345678";
    const String masterAscii = "1234567890123456";
    static Boolean staticInitialized;

    static AsnHexViewer() {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(AsnHexViewer),
            new FrameworkPropertyMetadata(typeof(AsnHexViewer)));
    }
    static void OnFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        ((AsnHexViewer)d).calculateWidths();
    }

    RichTextBox[] panes;

    Boolean controlInitialized, scrollLocked;
    TextRange[] ranges;

    ScrollBar Scroller;
    RichTextBox HexAddrHeaderRtb, HexRawHeaderRtb, HexAsciiHeaderRtb;
    BindableRichTextBox HexAddressPane, HexRawPane, HexAsciiPane;


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
        typeof(IList<Byte>),
        typeof(AsnHexViewer),
        new FrameworkPropertyMetadata(OnDataSourcePropertyChanged));
    public IList<Byte> DataSource {
        get => (IList<Byte>)GetValue(DataSourceProperty);
        set => SetValue(DataSourceProperty, value);
    }
    static void OnDataSourcePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e) {
        if (e.OldValue is INotifyCollectionChanged oldValue) {
            oldValue.CollectionChanged -= ((AsnHexViewer)source).OnCollectionChanged;
        }
        if (e.NewValue is INotifyCollectionChanged newValue) {
            newValue.CollectionChanged += ((AsnHexViewer)source).OnCollectionChanged;
        }
        ((AsnHexViewer)source).refreshView();
    }
    void OnCollectionChanged(Object o, NotifyCollectionChangedEventArgs e) {
        refreshView();
    }

    #endregion

    #region SelectedNode

    public static readonly DependencyProperty SelectedNodeProperty = DependencyProperty.Register(
        nameof(SelectedNode),
        typeof(IHexAsnNode),
        typeof(AsnHexViewer),
        new FrameworkPropertyMetadata(onSelectedNodeChanged));
    static void onSelectedNodeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
        var ctrl = (AsnHexViewer)sender;
        if (!ctrl.controlInitialized) {
            return;
        }
        if (e.OldValue is IHexAsnNode oldValue) {
            oldValue.DataChanged -= ctrl.onNodeDataChanged;
        }
        if (e.NewValue is null) {
            ctrl.ResetColors();
            return;
        }
        var treeNode = (IHexAsnNode)e.NewValue;
        ctrl.reColorHex(treeNode);
        treeNode.DataChanged += ctrl.onNodeDataChanged;
    }
    void reColorHex(IHexAsnNode treeNode) {
        if (!controlInitialized) {
            return;
        }
        ResetColors();
        ranges = GetSelectionPointers(treeNode, HexRawPane);
        Colorize();
        HexRawPane.CaretPosition = ranges[0].Start;
        scrollPanes(null);
    }
    void onNodeDataChanged(Object sender, EventArgs e) {
        if (sender is IHexAsnNode node) {
            reColorHex(node);
        }
    }


    public IHexAsnNode SelectedNode {
        get => (IHexAsnNode)GetValue(SelectedNodeProperty);
        set => SetValue(SelectedNodeProperty, value);
    }

    #endregion

    public static readonly DependencyProperty TagOctetBrushProperty = DependencyProperty.Register(
        nameof(TagOctetBrush),
        typeof(Brush),
        typeof(AsnHexViewer),
        new PropertyMetadata(default));

    public Brush TagOctetBrush {
        get => (Brush)GetValue(TagOctetBrushProperty);
        set => SetValue(TagOctetBrushProperty, value);
    }

    public static readonly DependencyProperty TagLengthOctetBrushProperty = DependencyProperty.Register(
        nameof(TagLengthOctetBrush),
        typeof(Brush),
        typeof(AsnHexViewer),
        new PropertyMetadata(default));

    public Brush TagLengthOctetBrush {
        get => (Brush)GetValue(TagLengthOctetBrushProperty);
        set => SetValue(TagLengthOctetBrushProperty, value);
    }

    public static readonly DependencyProperty TagPayloadOctetBrushProperty = DependencyProperty.Register(
        nameof(TagPayloadOctetBrush),
        typeof(Brush),
        typeof(AsnHexViewer),
        new PropertyMetadata(default));

    public Brush TagPayloadOctetBrush {
        get => (Brush)GetValue(TagPayloadOctetBrushProperty);
        set => SetValue(TagPayloadOctetBrushProperty, value);
    }


    void calculateWidths() {
        HexAddrHeaderRtb.SetWidthToFitString(masterAddr);
        HexRawHeaderRtb.SetWidthToFitString(masterHex);
        HexAsciiHeaderRtb.SetWidthToFitString(masterAscii);
    }
    void buildAddress() {
        var addressParagraph = new Paragraph();
        foreach (Int32 row in Enumerable.Range(0, (Int32)Math.Ceiling((Double)DataSource.Count / 16))) {
            addressParagraph.Inlines.Add(new Run($"{row * 16:X8}" + Environment.NewLine));
        }
        HexAddressPane.Document.Blocks.Clear();
        HexAddressPane.Document.Blocks.Add(addressParagraph);
    }
    void buildHex() {
        var hexParagraph = new Paragraph();
        hexParagraph.Inlines.Add(new Run(AsnFormatter.BinaryToString(DataSource.ToArray(), EncodingType.Hex).ToUpper()));
        HexRawPane.Document.Blocks.Clear();
        HexRawPane.Document.Blocks.Add(hexParagraph);
    }
    void buildAscii() {
        var asciiParagraph = new Paragraph();
        var SB = new StringBuilder();
        for (Int32 index = 0; index < DataSource.Count; index++) {
            Char c = DataSource[index] < 32 || DataSource[index] > 126
                ? '.'
                : (Char)DataSource[index];
            if (index != 0 && index % 16 == 0) {
                SB.Append(Environment.NewLine);
            }
            SB.Append(c);
        }
        asciiParagraph.Inlines.Add(new Run(SB + Environment.NewLine));
        HexAsciiPane.Document.Blocks.Clear();
        HexAsciiPane.Document.Blocks.Add(asciiParagraph);
    }
    public void onRtbScrollChanged(Object sender, ScrollChangedEventArgs e) {
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
        const Double smallStep = 48;
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

    void refreshView() {
        if (DataSource is null || !controlInitialized) {
            return;
        }

        buildAddress();
        buildHex();
        buildAscii();
    }

    public override void OnApplyTemplate() {
        Scroller = GetTemplateChild("PART_ScrollBar") as ScrollBar;
        if (Scroller is null) {
            throw new ArgumentException("'PART_ScrollBar' part was not found.");
        }
        Scroller.Maximum = 0;
        Scroller.Scroll += onScrollerScroll;

        HexAddrHeaderRtb = GetTemplateChild("PART_AddressHeader") as RichTextBox;
        HexRawHeaderRtb = GetTemplateChild("PART_HexHeader") as RichTextBox;
        HexAsciiHeaderRtb = GetTemplateChild("PART_AsciiHeader") as RichTextBox;

        HexAddressPane = initializeBindableRtb("PART_AddressBody");
        HexRawPane = initializeBindableRtb("PART_HexBody");
        HexAsciiPane = initializeBindableRtb("PART_AsciiBody");

        base.OnApplyTemplate();
        if (!staticInitialized) {
            FontSizeProperty.OverrideMetadata(typeof(AsnHexViewer), new FrameworkPropertyMetadata(OnFontSizeChanged));
            staticInitialized = true;
        }

        ranges = new TextRange[3];
        panes = [HexAddressPane, HexRawPane, HexAsciiPane];
        controlInitialized = true;
        calculateWidths();
        refreshView();
        if (SelectedNode is not null) {
            reColorHex(SelectedNode);
        }
    }

    BindableRichTextBox initializeBindableRtb(String resourceName) {
        var rtb = GetTemplateChild(resourceName) as BindableRichTextBox;
        rtb!.Loaded += (sender, _) => subscribeScrollViewerEvent((TextBoxBase)sender);
        rtb.Document = new FlowDocument();

        return rtb;
    }
    void subscribeScrollViewerEvent(TextBoxBase textBoxBase) {
        if (textBoxBase is null) {
            throw new ArgumentNullException(nameof(textBoxBase));
        }
        if (textBoxBase.Template.FindName("PART_ContentHost", textBoxBase) is not ScrollViewer scroll) {
            throw new ArgumentException("'PART_ContentHost' named element could not be found in current TextBoxBase instance.");
        }
        scroll.ScrollChanged += onRtbScrollChanged;
    }

    #region Hex Colorizer

    static Int32 getOffset(Int32 offset) {
        Int32 line = (Int32)Math.Floor((Double)offset / 16);
        return (offset - 16 * line) * 3 + 50 * line + 2;
    }
    static TextRange[] GetRanges(IList<TextPointer> pointers) {
        TextRange[] ranges =
        [
            new(pointers[0], pointers[1]),
            new(pointers[2], pointers[3]),
            new(pointers[4], pointers[5]),
        ];
        return ranges;
    }

    static TextRange[] GetSelectionPointers(IHexAsnNode treeNode, RichTextBox rtb) {
        TextPointer[] pointers =
        [
            // tag
            rtb.Document.ContentStart.GetPositionAtOffset(
                getOffset(treeNode.Offset)
            ),
            rtb.Document.ContentStart.GetPositionAtOffset(
                getOffset(treeNode.Offset + 1)
            ),
            // length bytes
            rtb.Document.ContentStart.GetPositionAtOffset(
                getOffset(treeNode.Offset + 1)
            ),
            rtb.Document.ContentStart.GetPositionAtOffset(
                getOffset(treeNode.PayloadStartOffset)
            ),
            // payload
            rtb.Document.ContentStart.GetPositionAtOffset(
                getOffset(treeNode.PayloadStartOffset)
            ),
            rtb.Document.ContentStart.GetPositionAtOffset(
                getOffset(treeNode.PayloadStartOffset + treeNode.PayloadLength)
            )
        ];
        return GetRanges(pointers);
    }
    void Colorize() {
        foreach (TextRange range in ranges) {
            range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
        }
        ranges[0].ApplyPropertyValue(TextElement.ForegroundProperty, TagOctetBrush);
        ranges[1].ApplyPropertyValue(TextElement.ForegroundProperty, TagLengthOctetBrush);
        ranges[2].ApplyPropertyValue(TextElement.ForegroundProperty, TagPayloadOctetBrush);
    }
    void ResetColors() {
        foreach (TextRange range in ranges.Where(range => range != null)) {
            range.ClearAllProperties();
        }
    }

    #endregion
}
