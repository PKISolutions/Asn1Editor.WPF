using System;
using System.Windows;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.API.ViewModel;

namespace SysadminsLV.Asn1Editor.Views.Windows {
    /// <summary>
    /// Interaction logic for TagDataEditor.xaml
    /// </summary>
    public partial class TagDataEditor {
        public TagDataEditor(Asn1Lite asn, Boolean hex = false) {
            InitializeComponent();
            ((TagDataEditorVM)DataContext).SetBinding(asn, hex);
        }

        void CancelClick(Object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
