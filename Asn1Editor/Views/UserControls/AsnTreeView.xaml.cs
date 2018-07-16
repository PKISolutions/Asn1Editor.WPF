using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.API.ViewModel;
using SysadminsLV.Asn1Editor.Views.Windows;
using Unity;

namespace SysadminsLV.Asn1Editor.Views.UserControls {
    /// <summary>
    /// Interaction logic for AsnTreeView.xaml
    /// </summary>
    public partial class AsnTreeView {
        readonly IDataSource _data;
        public AsnTreeView() {
            InitializeComponent();
            _data = App.Container.Resolve<IDataSource>();
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
        void OnTreeViewSelectedItemChanged(Object sender, RoutedPropertyChangedEventArgs<Object> e) {
            if (e.NewValue == null) { return; }
            _data.SelectedNode = e.NewValue as Asn1TreeNode;
        }
        void Tree_OnDrop(Object sender, DragEventArgs e) {
            String[] file = (String[])e.Data.GetData(DataFormats.FileDrop, true);
            if (file != null) {
                ((MainWindowVM)DataContext).DropFile(file[0]);
            }
        }

        static TreeViewItem VisualUpwardSearch(DependencyObject source) {
            while (source != null && !(source is TreeViewItem)) {
                source = VisualTreeHelper.GetParent(source);
            }
            return (TreeViewItem)source;
        }
    }
}
