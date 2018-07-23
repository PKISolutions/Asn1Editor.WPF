using System;
using System.Windows;
using SysadminsLV.Asn1Editor.API.Interfaces;

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
    }
}
