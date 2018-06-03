using System;
using System.Windows;

namespace Asn1Editor.Views.Windows {
    /// <summary>
    /// Interaction logic for TextViewer.xaml
    /// </summary>
    public partial class TextViewer {
        public TextViewer() {
            InitializeComponent();
        }

        void CloseClick(Object ender, RoutedEventArgs e) {
            Close();
        }
    }
}
