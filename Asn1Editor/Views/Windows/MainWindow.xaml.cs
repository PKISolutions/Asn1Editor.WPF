using System;
using System.ComponentModel;
using System.Windows;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.ViewModel;

namespace SysadminsLV.Asn1Editor.Views.Windows {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        public MainWindow(IMainWindowVM vm) {
            InitializeComponent();
            DataContext = vm;
        }

        void CloseClick(Object sender, RoutedEventArgs e) {
            Close();
        }
        void OnClosing(Object sender, CancelEventArgs e) {
            StaticCommands.ClearResources();
        }
    }
}
