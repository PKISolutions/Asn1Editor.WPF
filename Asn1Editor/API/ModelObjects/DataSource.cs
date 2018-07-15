using System;
using System.Collections.ObjectModel;
using SysadminsLV.Asn1Editor.API.Generic;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.ViewModel;

namespace SysadminsLV.Asn1Editor.API.ModelObjects {
    class DataSource : ViewModelBase, IDataSource {
        Asn1TreeNode selectedNode;

        public Asn1TreeNode SelectedNode {
            get => selectedNode;
            set {
                selectedNode = value;
                OnPropertyChanged(nameof(SelectedNode));
            }
        }
        public ObservableCollection<Asn1TreeNode> Tree { get; } = new ObservableCollection<Asn1TreeNode>();
        public ObservableList<Byte> RawData { get; } = new ObservableList<Byte> { IsNotifying = true };
    }
}
