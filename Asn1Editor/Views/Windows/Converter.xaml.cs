using System;
using System.Windows;

namespace Asn1Editor.Views.Windows {
    /// <summary>
    /// Interaction logic for Converter.xaml
    /// </summary>
    public partial class Converter {
        public Converter() {
            InitializeComponent();
        }

        void CloseClick(Object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
