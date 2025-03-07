using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.ViewModel;
using SysadminsLV.WPF.OfficeTheme.Toolkit.CLR;

namespace SysadminsLV.Asn1Editor.API.ModelObjects;

class DataSource(NodeViewOptions viewOptions) : ViewModelBase, IDataSource {
    readonly ObservableCollection<Asn1TreeNode> _tree = [];
    Asn1TreeNode selectedNode;

    protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
        if (RawData.IsNotifying && CollectionChanged != null) {
            try {
                CollectionChanged(this, e);
            } catch (NotSupportedException) {
                var alternativeEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                OnCollectionChanged(alternativeEventArgs);
            }
        }
    }
    public Asn1TreeNode SelectedNode {
        get => selectedNode;
        set {
            selectedNode = value;
            OnPropertyChanged(nameof(SelectedNode));
        }
    }
    public NodeViewOptions NodeViewOptions { get; } = viewOptions;
    public ReadOnlyObservableCollection<Asn1TreeNode> Tree => new(_tree);
    public void SetRootNode(Asn1TreeNode node) {
        _tree.Clear();
        _tree.Add(node);
        FinishBinaryUpdate();
    }
    public void FinishBinaryUpdate() {
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        if (Tree.Count > 0) {
            Tree[0].UpdateNodeView();
        }
    }
    public ObservableList<Byte> RawData { get; } = new(true, false);

    public void Reset() {
        _tree.Clear();
        SelectedNode = null;
        RawData.Clear();
    }

    public event NotifyCollectionChangedEventHandler CollectionChanged;
}