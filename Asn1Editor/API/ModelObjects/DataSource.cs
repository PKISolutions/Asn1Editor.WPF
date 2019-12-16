using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using SysadminsLV.Asn1Editor.API.Generic;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.ViewModel;

namespace SysadminsLV.Asn1Editor.API.ModelObjects {
    class DataSource : ViewModelBase, IDataSource {
        Asn1TreeNode selectedNode;
        Boolean hasClipboard;

        public DataSource(NodeViewOptions viewOptions) {
            NodeViewOptions = viewOptions;
        }

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
        public Boolean HasClipboardData {
            get => hasClipboard;
            set {
                hasClipboard = value;
                OnPropertyChanged(nameof(HasClipboardData));
            }
        }
        public Asn1TreeNode SelectedNode {
            get => selectedNode;
            set {
                selectedNode = value;
                OnPropertyChanged(nameof(SelectedNode));
            }
        }
        public NodeViewOptions NodeViewOptions { get; }
        public ObservableCollection<Asn1TreeNode> Tree { get; } = new ObservableCollection<Asn1TreeNode>();
        public void FinishBinaryUpdate() {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            if (Tree.Count > 0) {
                Tree[0].UpdateNodeView(NodeViewOptions);
            }
        }
        public ObservableList<Byte> RawData { get; } = new ObservableList<Byte> { IsNotifying = true };

        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}
