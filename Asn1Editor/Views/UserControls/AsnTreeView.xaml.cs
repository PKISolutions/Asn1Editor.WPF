using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.API.ViewModel;
using SysadminsLV.Asn1Editor.Views.Windows;

namespace SysadminsLV.Asn1Editor.Views.UserControls {
    /// <summary>
    /// Interaction logic for AsnTreeView.xaml
    /// </summary>
    public partial class AsnTreeView {
        public AsnTreeView() {
            InitializeComponent();
        }

        void TreeViewDoubleClick(Object sender, MouseButtonEventArgs e) {
            if (tree.SelectedItem == null) { return; }
            TagDataEditor dlg = new TagDataEditor(((Asn1TreeNode)tree.SelectedItem).Value);
            dlg.ShowDialog();
        }
        void OnPreviewMouseRightButtonDown(Object sender, MouseButtonEventArgs e) {
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);
            if (treeViewItem != null) {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }
        void TreeViewSelectedItemChanged(Object sender, RoutedPropertyChangedEventArgs<Object> e) {
            if (e.NewValue == null) { return; }
            ((MainWindowVM)DataContext).SelectedTreeNode = (Asn1TreeNode)e.NewValue;
        }
        void Tree_OnDrop(Object sender, DragEventArgs e) {
            String[] file = (String[])e.Data.GetData(DataFormats.FileDrop, true);
            ((MainWindowVM)DataContext).DropFile(file[0]);
        }

        static TreeViewItem VisualUpwardSearch(DependencyObject source) {
            while (source != null && !(source is TreeViewItem)) {
                source = VisualTreeHelper.GetParent(source);
            }
            return (TreeViewItem)source;
        }
    }
}
