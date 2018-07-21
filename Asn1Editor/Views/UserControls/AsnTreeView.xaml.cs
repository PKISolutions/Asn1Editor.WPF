using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SysadminsLV.Asn1Editor.API.ViewModel;
using SysadminsLV.WPF.OfficeTheme.Toolkit.Commands;

namespace SysadminsLV.Asn1Editor.Views.UserControls {
    /// <summary>
    /// Interaction logic for AsnTreeView.xaml
    /// </summary>
    public partial class AsnTreeView {
        public AsnTreeView() {
            InitializeComponent();
            Focusable = true;
            ExpandAllCommand = new RelayCommand(expandAll);
        }

        public static readonly DependencyProperty ExpandAllCommandProperty = DependencyProperty.Register(
            nameof(ExpandAllCommand), typeof(ICommand), typeof(AsnTreeView), new PropertyMetadata(default(ICommand)));

        public ICommand ExpandAllCommand {
            get => (ICommand)GetValue(ExpandAllCommandProperty);
            private set => SetValue(ExpandAllCommandProperty, value);
        }

        public static readonly DependencyProperty DoubleClickCommandProperty = DependencyProperty.Register(
            nameof(DoubleClickCommand),
            typeof(ICommand),
            typeof(AsnTreeView),
            new PropertyMetadata(default(ICommand)));

        public ICommand DoubleClickCommand {
            get => (ICommand)GetValue(DoubleClickCommandProperty);
            set => SetValue(DoubleClickCommandProperty, value);
        }

        public static readonly DependencyProperty DoubleClickCommandParameterProperty = DependencyProperty.Register(
            nameof(DoubleClickCommandParameter),
            typeof(Object),
            typeof(AsnTreeView),
            new PropertyMetadata(default(Object)));

        public Object DoubleClickCommandParameter {
            get => GetValue(DoubleClickCommandParameterProperty);
            set => SetValue(DoubleClickCommandParameterProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(Object),
            typeof(AsnTreeView),
            new PropertyMetadata(default(Object)));

        public Object SelectedItem {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        void OnTreeViewDoubleClick(Object sender, MouseButtonEventArgs e) {
            DoubleClickCommand.Execute(DoubleClickCommandParameter);
        }
        void OnPreviewMouseRightButtonDown(Object sender, MouseButtonEventArgs e) {
            TreeViewItem treeViewItem = visualUpwardSearch(e.OriginalSource as DependencyObject);
            if (treeViewItem != null) {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }
        void OnTreeViewSelectedItemChanged(Object sender, RoutedPropertyChangedEventArgs<Object> e) {
            if (e.NewValue == null) { return; }
            SelectedItem = e.NewValue;
        }
        void Tree_OnDrop(Object sender, DragEventArgs e) {
            String[] file = (String[])e.Data.GetData(DataFormats.FileDrop, true);
            if (file != null) {
                ((MainWindowVM)DataContext).DropFile(file[0]);
            }
        }
        void expandAll(Object o) {
            ((TreeViewItem)tree.Items[0]).IsExpanded = true;
        }

        static TreeViewItem visualUpwardSearch(DependencyObject source) {
            while (source != null && !(source is TreeViewItem)) {
                source = VisualTreeHelper.GetParent(source);
            }
            return (TreeViewItem)source;
        }
    }
}
