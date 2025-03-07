using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
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

    BindableRichTextBox[] panes;

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
            TextUtility.ResetColors(ctrl.ranges);
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
        TextUtility.ResetColors(ranges);
        ranges = TextUtility.GetSelectionPointers(treeNode, HexRawPane);
        TextUtility.Colorize(ranges);
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

    void calculateWidths() {
        HexAddrHeaderRtb.MeasureStringWidth(masterAddr);
        HexRawHeaderRtb.MeasureStringWidth(masterHex);
        HexAsciiHeaderRtb.MeasureStringWidth(masterAscii);
    }
    void buildAddress() {
        HexAddressPane.Document = new FlowDocument();
        var addressParagraph = new Paragraph();
        foreach (Int32 row in Enumerable.Range(0, (Int32)Math.Ceiling((Double)DataSource.Count / 16))) {
            addressParagraph.Inlines.Add(new Run($"{row * 16:X8}" + Environment.NewLine));
        }
        HexAddressPane.Document.Blocks.Add(addressParagraph);
    }
    void buildHex() {
        HexRawPane.Document = new FlowDocument();
        var hexParagraph = new Paragraph();
        hexParagraph.Inlines.Add(new Run(AsnFormatter.BinaryToString(DataSource.ToArray(), EncodingType.Hex).ToUpper()));
        HexRawPane.Document.Blocks.Add(hexParagraph);
    }
    void buildAscii() {
        HexAsciiPane.Document = new FlowDocument();
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

        HexAddressPane = GetTemplateChild("PART_AddressBody") as BindableRichTextBox;
        HexAddressPane!.Loaded += (sender, _) => subscribeScrollViewerEvent((TextBoxBase)sender);
        HexRawPane = GetTemplateChild("PART_HexBody") as BindableRichTextBox;
        HexRawPane!.Loaded += (sender, _) => subscribeScrollViewerEvent((TextBoxBase)sender);
        HexAsciiPane = GetTemplateChild("PART_AsciiBody") as BindableRichTextBox;
        HexAsciiPane!.Loaded += (sender, _) => subscribeScrollViewerEvent((TextBoxBase)sender);

        base.OnApplyTemplate();
        if (!staticInitialized) {
            FontSizeProperty.OverrideMetadata(typeof(AsnHexViewer), new FrameworkPropertyMetadata(OnFontSizeChanged));
            staticInitialized = true;
        }

        ranges = new TextRange[3];
        calculateWidths();
        panes = [HexAddressPane, HexRawPane, HexAsciiPane];
        controlInitialized = true;
        refreshView();
        if (SelectedNode is not null) {
            reColorHex(SelectedNode);
        }
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
}
