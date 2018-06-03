using System;
using System.Windows;
using System.Windows.Documents;
using Asn1Editor.API.ModelObjects;
using Asn1Editor.API.Utils;
using Asn1Editor.API.ViewModel;

namespace Asn1Editor.Views.UserControls {
    /// <summary>
    /// Interaction logic for HexViewerUC.xaml
    /// </summary>
    partial class HexViewerUC : IHexView {
        TextRange[] ranges;
        public HexViewerUC() {
            InitializeComponent();
            ranges = new TextRange[3];
        }

        void HexViewerUC_OnLoaded(Object sender, RoutedEventArgs e) {
            ((MainWindowVM)DataContext).HexViewerContext.View = this;
        }

        public void SelectHexView(Asn1Lite treeNode) {
            TextUtility.ResetColors(ranges);
            ranges = TextUtility.GetTextPointers(treeNode, hexTextBox, Scroller);
            TextUtility.Colorize(ranges);
        }
    }
}
