using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.WPF.OfficeTheme.Toolkit.Commands;

namespace SysadminsLV.Asn1Editor.Views.UserControls;

/// <summary>
/// Interaction logic for AsnTreeView.xaml
/// </summary>
public partial class AsnTreeView {
    public AsnTreeView() {
        InitializeComponent();
        Focusable = true;
        ExpandAllCommand = new RelayCommand(expandAll);
    }

    #region ExpandAllCommand

    public static readonly DependencyProperty ExpandAllCommandProperty = DependencyProperty.Register(
        nameof(ExpandAllCommand),
        typeof(ICommand),
        typeof(AsnTreeView),
        new PropertyMetadata(default(ICommand)));

    public ICommand ExpandAllCommand {
        get => (ICommand)GetValue(ExpandAllCommandProperty);
        private set => SetValue(ExpandAllCommandProperty, value);
    }

    #endregion

    #region FileDropCommand

    public static readonly DependencyProperty FileDropCommandProperty = DependencyProperty.Register(
        nameof(FileDropCommand),
        typeof(ICommand),
        typeof(AsnTreeView),
        new PropertyMetadata(default(ICommand)));

    public ICommand FileDropCommand {
        get => (ICommand)GetValue(FileDropCommandProperty);
        set => SetValue(FileDropCommandProperty, value);
    }

    #endregion

    #region DoubleClickCommand

    public static readonly DependencyProperty DoubleClickCommandProperty = DependencyProperty.Register(
        nameof(DoubleClickCommand),
        typeof(ICommand),
        typeof(AsnTreeView),
        new PropertyMetadata(default(ICommand)));

    public ICommand DoubleClickCommand {
        get => (ICommand)GetValue(DoubleClickCommandProperty);
        set => SetValue(DoubleClickCommandProperty, value);
    }

    #endregion

    #region DoubleClickCommandParameter

    public static readonly DependencyProperty DoubleClickCommandParameterProperty = DependencyProperty.Register(
        nameof(DoubleClickCommandParameter),
        typeof(Object),
        typeof(AsnTreeView),
        new PropertyMetadata(default(Object)));

    public Object DoubleClickCommandParameter {
        get => GetValue(DoubleClickCommandParameterProperty);
        set => SetValue(DoubleClickCommandParameterProperty, value);
    }

    #endregion

    #region SelectedItem

    public new static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem),
        typeof(Object),
        typeof(AsnTreeView),
        new PropertyMetadata(default, onSelectedItemChanged));
    static void onSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (e.NewValue is Asn1TreeNode node) {
            node.IsSelected = true;
        }
    }

    public new Object SelectedItem {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    #endregion

    #region MaxTextLength

    public static readonly DependencyProperty MaxTextLengthProperty = DependencyProperty.Register(
        nameof(MaxTextLength),
        typeof(Int32),
        typeof(AsnTreeView),
        new PropertyMetadata(default(Int32)));

    public Int32 MaxTextLength {
        get => (Int32)GetValue(MaxTextLengthProperty);
        set => SetValue(MaxTextLengthProperty, value);
    }

    #endregion

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
        SelectedItem = e.NewValue;
    }
    void onTreeFileDrop(Object sender, DragEventArgs e) {
        if (!AllowDrop) {
            e.Handled = true;
            return;
        }
        String[] file = (String[])e.Data.GetData(DataFormats.FileDrop, true);
        if (FileDropCommand != null && file?.Length > 0) {
            FileDropCommand.Execute(file[0]);
            e.Handled = true;
        }
    }
    void expandAll(Object o) {
        ((TreeViewItem)tree.Items[0]).IsExpanded = true;
    }

    static TreeViewItem visualUpwardSearch(DependencyObject source) {
        while (source != null && source is not TreeViewItem) {
            source = VisualTreeHelper.GetParent(source);
        }
        return (TreeViewItem)source;
    }
}