using System;
using System.Windows;

namespace SysadminsLV.Asn1Editor.Views.Windows {
    /// <summary>
    /// Interaction logic for LicenseWindow.xaml
    /// </summary>
    public partial class LicenseWindow {
        public LicenseWindow() {
            InitializeComponent();
        }
        void CloseClick(Object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
