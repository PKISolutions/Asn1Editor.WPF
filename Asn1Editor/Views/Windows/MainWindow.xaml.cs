using System;
using System.ComponentModel;
using System.Windows;
using SysadminsLV.Asn1Editor.API.Interfaces;

namespace SysadminsLV.Asn1Editor.Views.Windows; 

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow {
    readonly IMainWindowVM _vm;
    public MainWindow(IMainWindowVM vm) {
        _vm = vm;
        InitializeComponent();
        DataContext = vm;
        Closing += onClosing;
    }
    void onClosing(Object sender, CancelEventArgs e) {
        if (_vm.IsModified) {
            if (!_vm.RequestFileSave()) {
                e.Cancel = true;
            }
        }
    }

    void CloseClick(Object sender, RoutedEventArgs e) {
        Close();
    }
}