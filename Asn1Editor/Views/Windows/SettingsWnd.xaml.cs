using System;
using System.Windows;

namespace SysadminsLV.Asn1Editor.Views.Windows {
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWnd {
        public SettingsWnd() {
            InitializeComponent();
        }

        void CloseClick(Object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
