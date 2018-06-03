using System;
using System.Windows;
using System.Windows.Controls;

namespace SysadminsLV.Asn1Editor.Controls {
    public static class CaretBehavior {
        static void OnObserveCaretPropertyChanged(DependencyObject dpo, DependencyPropertyChangedEventArgs e) {
            if (dpo is TextBox textBox) {
                if ((Boolean)e.NewValue) {
                    textBox.SelectionChanged += TextBoxSelectionChanged;
                } else {
                    textBox.SelectionChanged -= TextBoxSelectionChanged;
                }
            }
        }
        static void TextBoxSelectionChanged(Object sender, RoutedEventArgs e) {
            if (sender is TextBox textBox) {
                Int32 line = textBox.GetLineIndexFromCharacterIndex(textBox.SelectionStart);
                SetCaretIndex(textBox, textBox.SelectionStart - textBox.GetCharacterIndexFromLineIndex(line) + 1);
                SetLineIndex(textBox, line + 1);
            }
        }
        static readonly DependencyProperty LineIndexProperty = DependencyProperty.RegisterAttached(
            "LineIndex",
            typeof(Int32),
            typeof(CaretBehavior));
        static readonly DependencyProperty CaretIndexProperty = DependencyProperty.RegisterAttached(
            "CaretIndex",
            typeof(Int32),
            typeof(CaretBehavior));

        public static readonly DependencyProperty ObserveCaretProperty = DependencyProperty.RegisterAttached(
            "ObserveCaret",
            typeof(Boolean),
            typeof(CaretBehavior),
            new UIPropertyMetadata(false, OnObserveCaretPropertyChanged));
        public static Boolean GetObserveCaret(DependencyObject obj) {
            return (Boolean)obj.GetValue(ObserveCaretProperty);
        }
        public static void SetObserveCaret(DependencyObject obj, Boolean value) {
            obj.SetValue(ObserveCaretProperty, value);
        }
        public static void SetCaretIndex(DependencyObject element, Int32 value) {
            element.SetValue(CaretIndexProperty, value);
        }
        public static Int32 GetCaretIndex(DependencyObject element) {
            return (Int32)element.GetValue(CaretIndexProperty);
        }
        public static void SetLineIndex(DependencyObject element, Int32 value) {
            element.SetValue(LineIndexProperty, value);
        }
        public static Int32 GetLineIndex(DependencyObject element) {
            return (Int32)element.GetValue(LineIndexProperty);
        }
    }
}
