using System;
using System.Windows;
using System.Windows.Controls;
using Asn1Editor.Wpf.Controls.Helpers;

namespace Asn1Editor.Wpf.Controls;

/// <summary>
/// Represents a textbox control that can set its width to fit exact number of characters.
/// </summary>
public class AutoSizeTextBox : TextBox {
    static AutoSizeTextBox() {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(AutoSizeTextBox),
            new FrameworkPropertyMetadata(typeof(AutoSizeTextBox)));
        FontSizeProperty.OverrideMetadata(typeof(AutoSizeTextBox),
            new FrameworkPropertyMetadata(OnFontSizeChanged));
    }

    static void OnFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        ((AutoSizeTextBox)d).recalculateWidth();
    }

    public static readonly DependencyProperty LineWidthProperty = DependencyProperty.Register(
        nameof(LineWidth),
        typeof(Int32),
        typeof(AutoSizeTextBox),
        new PropertyMetadata(0, onLineWidthChanged));
    static void onLineWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        ((AutoSizeTextBox)d).recalculateWidth();
    }

    public Int32 LineWidth {
        get => (Int32)GetValue(LineWidthProperty);
        set => SetValue(LineWidthProperty, value);
    }

    void recalculateWidth() {
        this.MeasureStringWidth(new String('M', LineWidth));
    }
}
