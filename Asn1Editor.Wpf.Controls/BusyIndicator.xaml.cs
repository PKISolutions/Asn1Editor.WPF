using System;
using System.Windows;

namespace Asn1Editor.Wpf.Controls;

/// <summary>
/// Interaction logic for BusyIndicator.xaml
/// </summary>
public partial class BusyIndicator {
    public BusyIndicator() {
        InitializeComponent();
        IsVisibleChanged += OnIsVisibleChanged;
    }

    void OnIsVisibleChanged(Object Sender, DependencyPropertyChangedEventArgs e) {
        pb.IsIndeterminate = (Boolean)e.NewValue;
    }
}