using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Asn1Editor.Controls {
    public class BindableRichTextBox : RichTextBox {
        public static readonly DependencyProperty DocumentProperty = DependencyProperty.Register(
            nameof(Document),
            typeof(FlowDocument),
            typeof(BindableRichTextBox),
            new FrameworkPropertyMetadata(null, OnDocumentChanged));
        public static readonly DependencyProperty CurrentLineProperty = DependencyProperty.Register(
            nameof(CurrentLine),
            typeof(Int32),
            typeof(BindableRichTextBox),
            new FrameworkPropertyMetadata(1));
        public static readonly DependencyProperty CurrentColumnProperty = DependencyProperty.Register(
            nameof(CurrentColumn),
            typeof(Int32),
            typeof(BindableRichTextBox),
            new FrameworkPropertyMetadata(1));
        public static readonly DependencyProperty ObserveCaretProperty = DependencyProperty.Register(
            nameof(ObserveCaret),
            typeof(Boolean),
            typeof(BindableRichTextBox),
            new UIPropertyMetadata(false, OnObserveCaretPropertyChanged));

        static void OnObserveCaretPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            RichTextBox rtb = (RichTextBox)d;
            if (rtb != null) {
                if ((Boolean)e.NewValue) {
                    rtb.SelectionChanged += RtbSelectionChanged;
                } else {
                    rtb.SelectionChanged -= RtbSelectionChanged;
                }
            }
        }
        static void RtbSelectionChanged(Object sender, RoutedEventArgs e) {
            BindableRichTextBox rtb = (BindableRichTextBox)sender;
            if (rtb == null) { return; }

            rtb.CaretPosition.GetLineStartPosition(-Int32.MaxValue, out Int32 lineNumber);
            rtb.CurrentLine = -lineNumber + 1;
            TextPointer lineStartPosition = rtb.CaretPosition.GetLineStartPosition(0);
            if (lineStartPosition != null) {
                rtb.CurrentColumn = lineNumber == 0
                    ? lineStartPosition.GetOffsetToPosition(rtb.CaretPosition)
                    : lineStartPosition.GetOffsetToPosition(rtb.CaretPosition) - 1;
            }
        }

        public new FlowDocument Document {
            get => (FlowDocument)GetValue(DocumentProperty);
            set => SetValue(DocumentProperty, value);
        }
        public Boolean ObserveCaret {
            get => (Boolean)GetValue(ObserveCaretProperty);
            set => SetValue(ObserveCaretProperty, value);
        }
        public Int32 CurrentLine {
            get => (Int32)GetValue(CurrentLineProperty);
            set => SetValue(CurrentLineProperty, value);
        }
        public Int32 CurrentColumn {
            get => (Int32)GetValue(CurrentColumnProperty);
            set => SetValue(CurrentColumnProperty, value);
        }

        public static void OnDocumentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            RichTextBox rtb = (RichTextBox)obj;
            rtb.Document = (FlowDocument)args.NewValue;
        }
    }
}
